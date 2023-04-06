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
    using BasicSalesSystem.Web.Requests.Purchase;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class PurchasesController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public PurchasesController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetPurchasesList(GetPurchasesListRequest request)
        {
            var purchases = _mongoDbService.GetCollection<Purchase>(Collections.Purchases).AsQueryable();
            var suppliers = _mongoDbService.GetCollection<Supplier>(Collections.Suppliers).AsQueryable();

            var query = from p in purchases
                        join c in suppliers on p.SupplierId equals c.Id into joined
                        select new { Purchase = p, Suppliers = joined };

            if (!request.StartDate.Equals(DateTime.MinValue))
            {
                query = query.Where(item => item.Purchase.Date >= request.StartDate.Date);
            }

            if (!request.EndDate.Equals(DateTime.MinValue))
            {
                query = query.Where(item => item.Purchase.Date <= request.EndDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(request.SupplierId))
            {
                query = query.Where(x => x.Purchase.SupplierId.Equals(request.SupplierId));
            }

            var totalItemCount = await query.CountAsync();
            var items = await query
                .ApplyOrdering(request.SortBy, request.SortDesc)
                .ApplyPaging(request.Page, request.PageSize)
                .ToListAsync();

            var productsIds = items.SelectMany(i => i.Purchase.Details.Select(d => d.ProductId)).Distinct().ToList();
            var productFilter = Builders<Product>.Filter.In(x => x.Id, productsIds);
            var products = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).ToListAsync();

            var resources = new PagedListResource()
            {
                TotalItemCount = totalItemCount,
                ItemsList = items
                    .Select(item =>
                    {
                        var supplier = item.Suppliers.FirstOrDefault();
                        return new
                        {
                            item.Purchase.Id,
                            Supplier = new
                            {
                                supplier.Id,
                                supplier.FullName
                            },
                            item.Purchase.ReceiptType,
                            item.Purchase.Tax,
                            item.Purchase.Total,
                            item.Purchase.Date,
                            item.Purchase.Approved,
                            Details = item.Purchase.Details.Select(d =>
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
                                    d.UnitPrice
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
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetAnnualStatistics()
        {
            var purchases = _mongoDbService.GetCollection<Purchase>(Collections.Purchases).AsQueryable();

            var query = from p in purchases
                        where p.Approved
                        group p by new { p.Date.Year, p.Date.Month } into grouped
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
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
        {
            var supplierFilter = Builders<Supplier>.Filter.Where(x => x.Id.Equals(request.SupplierId));
            var validSupplier = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(supplierFilter).AnyAsync();

            if (!validSupplier)
            {
                throw new CustomException("Invalid supplier specified.");
            }

            var productFilter = Builders<Product>.Filter.In(x => x.Id, request.Details.Select(d => d.ProductId));
            var products = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).ToListAsync();

            var validProducts = request.Details
                .All(d => products.Any(p => p.Id.Equals(d.ProductId)));

            if (!validProducts)
            {
                throw new CustomException("Invalid products specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var newPurchase = new Purchase()
            {
                SupplierId = request.SupplierId,
                ReceiptType = request.ReceiptType.Trim().ToLowerInvariant(),
                Tax = request.Tax,
                Total = Math.Round(request.Details.Sum(d => d.Quantity * d.UnitPrice) * (1 + (request.Tax / 100)), 2),
                Date = DateTime.Now,
                Approved = true,
                Details = request.Details.Select(d => new PurchaseDetail()
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                })
                .ToList()
            };

            await _mongoDbService.GetCollection<Purchase>(Collections.Purchases).InsertOneAsync(session, newPurchase);
            await request.Details.ForEachAsync(async item =>
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

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> CancelPurchase(String id)
        {
            var purchaseFilter = Builders<Purchase>.Filter.Where(x => x.Id.Equals(id) && x.Approved);
            var purchase = await _mongoDbService.GetCollection<Purchase>(Collections.Purchases)
                .Find(purchaseFilter).FirstOrDefaultAsync();

            if (purchase == null)
            {
                throw new CustomException("Invalid purchase specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Purchase>()
                .Set(x => x.Approved, false);

            await _mongoDbService.GetCollection<Purchase>(Collections.Purchases).UpdateOneAsync(session, purchaseFilter, updateDefinition);
            await purchase.Details.ForEachAsync(async item =>
            {
                var productFilter = Builders<Product>.Filter.Where(p => p.Id.Equals(item.ProductId));
                var product = await _mongoDbService.GetCollection<Product>(Collections.Products)
                    .Find(productFilter).FirstOrDefaultAsync();

                if (product.Stock < item.Quantity)
                {
                    throw new CustomException("Purchase cannot be cancelled due to insufficient stock.");
                }

                var stockUpdateDefinition = new UpdateDefinitionBuilder<Product>()
                    .Inc(x => x.Stock, -item.Quantity);

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
