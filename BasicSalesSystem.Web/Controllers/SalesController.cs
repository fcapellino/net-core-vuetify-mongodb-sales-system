namespace BasicSalesSystem.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BasicSalesSystem.Web.Custom;
    using BasicSalesSystem.Web.Custom.Enumerations;
    using BasicSalesSystem.Web.Custom.Resources;
    using BasicSalesSystem.Web.Dependencies.MongoDbService;
    using BasicSalesSystem.Web.Domain.Entities;
    using BasicSalesSystem.Web.Requests.Sale;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class SalesController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public SalesController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> GetSalesList(GetSalesListRequest request)
        {
            var sales = _mongoDbService.GetCollection<Sale>(Collections.Sales).AsQueryable();
            var customers = _mongoDbService.GetCollection<Customer>(Collections.Customers).AsQueryable();

            var query = from s in sales
                        join c in customers on s.CustomerId equals c.Id into joined
                        select new { Sale = s, Customers = joined };

            if (!request.StartDate.Equals(DateTime.MinValue))
            {
                query = query.Where(item => item.Sale.Date >= request.StartDate.Date);
            }

            if (!request.EndDate.Equals(DateTime.MinValue))
            {
                query = query.Where(item => item.Sale.Date <= request.EndDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(request.CustomerId))
            {
                query = query.Where(x => x.Sale.CustomerId.Equals(request.CustomerId));
            }

            var totalItemCount = await query.CountAsync();
            var items = await query
                .ApplyOrdering(request.SortBy, request.SortDesc)
                .ApplyPaging(request.Page, request.PageSize)
                .ToListAsync();

            var productsIds = items.SelectMany(i => i.Sale.Details.Select(d => d.ProductId)).Distinct().ToList();
            var productFilter = Builders<Product>.Filter.In(x => x.Id, productsIds);
            var products = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).ToListAsync();

            var resources = new PagedListResource()
            {
                TotalItemCount = totalItemCount,
                ItemsList = items
                    .Select(item =>
                    {
                        var customer = item.Customers.FirstOrDefault();
                        return new
                        {
                            item.Sale.Id,
                            Customer = new
                            {
                                customer.Id,
                                customer.FullName
                            },
                            item.Sale.ReceiptType,
                            item.Sale.Tax,
                            item.Sale.Total,
                            item.Sale.Date,
                            item.Sale.Approved,
                            Details = item.Sale.Details.Select(d =>
                            {
                                var product = products.FirstOrDefault(p => p.Id.Equals(d.ProductId));
                                return new
                                {
                                    d.Id,
                                    Product = new
                                    {
                                        product.Id,
                                        product.Name
                                    },
                                    d.Quantity,
                                    d.UnitPrice,
                                    d.Discount
                                };
                            })
                            .OrderBy(p => p.Product.Name)
                            .ToList()
                        };
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> GetAnnualStatistics()
        {
            var sales = _mongoDbService.GetCollection<Sale>(Collections.Sales).AsQueryable();

            var query = from s in sales
                        where s.Approved
                        group s by new { s.Date.Year, s.Date.Month } into grouped
                        orderby grouped.Key.Year ascending, grouped.Key.Month ascending
                        select new { Date = grouped.Key, Total = grouped.Sum(p => p.Total) };

            var items = await query
                .ApplyPaging(1, 12)
                .ToListAsync();

            var resources = new ListResource()
            {
                ItemsList = items
                    .Select(item =>
                    {
                        return new
                        {
                            Label = $"{item.Date.Year}-{item.Date.Month}",
                            item.Total
                        };
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
        {
            var customerFilter = Builders<Customer>.Filter.Where(x => x.Id.Equals(request.CustomerId));
            var validCustomer = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(customerFilter).AnyAsync();

            if (!validCustomer)
            {
                throw new CustomException("Invalid customer specified.");
            }

            var productFilter = Builders<Product>.Filter.In(x => x.Id, request.Details.Select(d => d.ProductId));
            var products = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).ToListAsync();

            var validProducts = request.Details
                .All(d => products.Any(p => p.Id.Equals(d.ProductId) && p.Stock >= d.Quantity));

            if (!validProducts)
            {
                throw new CustomException("Invalid products specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var newSale = new Sale()
            {
                CustomerId = request.CustomerId,
                ReceiptType = request.ReceiptType.Trim().ToLowerInvariant(),
                Tax = request.Tax,
                Total = Math.Round(request.Details.Sum(d => d.Quantity * d.UnitPrice * (1 - (d.Discount / 100))) * (1 + (request.Tax / 100)), 2),
                Date = DateTime.Now,
                Approved = true,
                Details = request.Details.Select(d => new SaleDetail()
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount
                })
                .ToList()
            };

            await _mongoDbService.GetCollection<Sale>(Collections.Sales).InsertOneAsync(session, newSale);
            await request.Details.ForEachAsync(async item =>
            {
                var productFilter = Builders<Product>.Filter.Where(p => p.Id.Equals(item.ProductId));
                var stockUpdateDefinition = new UpdateDefinitionBuilder<Product>()
                    .Inc(x => x.Stock, -item.Quantity);

                await _mongoDbService.GetCollection<Product>(Collections.Products)
                    .UpdateOneAsync(session, productFilter, stockUpdateDefinition);
            });

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> CancelSale(String id)
        {
            var saleFilter = Builders<Sale>.Filter.Where(x => x.Id.Equals(id) && x.Approved);
            var sale = await _mongoDbService.GetCollection<Sale>(Collections.Sales)
                .Find(saleFilter).FirstOrDefaultAsync();

            if (sale == null)
            {
                throw new CustomException("Invalid sale specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Sale>()
                .Set(x => x.Approved, false);

            await _mongoDbService.GetCollection<Sale>(Collections.Sales).UpdateOneAsync(session, saleFilter, updateDefinition);
            await sale.Details.ForEachAsync(async item =>
            {
                var productFilter = Builders<Product>.Filter.Where(p => p.Id.Equals(item.ProductId));
                var stockUpdateDefinition = new UpdateDefinitionBuilder<Product>()
                    .Inc(x => x.Stock, item.Quantity);

                await _mongoDbService.GetCollection<Product>(Collections.Products)
                    .UpdateOneAsync(session, productFilter, stockUpdateDefinition);
            });

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _mongoDbService?.Dispose();
        }
    }
}
