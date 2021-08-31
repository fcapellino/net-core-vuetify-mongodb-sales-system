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
    using BasicSalesSystem.Web.Requests.Product;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public ProductsController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetProductsList(GetProductsListRequest request)
        {
            var products = _mongoDbService.GetCollection<Product>(Collections.Products).AsQueryable();
            var categories = _mongoDbService.GetCollection<Category>(Collections.Categories).AsQueryable();

            var query = from p in products
                        join c in categories on p.CategoryId equals c.Id into joined
                        select new { Product = p, Categories = joined };

            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                query = query.Where(x => x.Product.CategoryId.Equals(request.CategoryId));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                query = query.Where(x => x.Product.Name.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                                         x.Product.Description.ToLower().Contains(request.SearchQuery.ToLower().Trim()));
            }

            var totalItemCount = await query.CountAsync();
            var items = await query
                .ApplyOrdering(request.SortBy, request.SortDesc)
                .ApplyPaging(request.Page, request.PageSize)
                .ToListAsync();

            var resources = new PagedListResource()
            {
                TotalItemCount = totalItemCount,
                ItemsList = items
                    .Select(item =>
                    {
                        var category = item.Categories.FirstOrDefault();
                        return new
                        {
                            item.Product.Id,
                            Category = new
                            {
                                category.Id,
                                category.Name,
                                category.Active
                            },
                            item.Product.Name,
                            item.Product.Description,
                            item.Product.Stock,
                            item.Product.UnitPrice,
                            item.Product.Base64Image,
                            item.Product.Active
                        };
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateUpdateProductRequest request)
        {
            var categoryFilter = Builders<Category>.Filter.Where(x => x.Id.Equals(request.CategoryId));
            var validCategory = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(categoryFilter).AnyAsync();

            if (!validCategory)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var newProduct = new Product()
            {
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Stock = request.Stock,
                UnitPrice = request.UnitPrice,
                Base64Image = request.Base64Image,
                Active = true
            };

            await _mongoDbService.GetCollection<Product>(Collections.Products).InsertOneAsync(session, newProduct);
            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> UpdateProduct([FromBody] CreateUpdateProductRequest request)
        {
            var productFilter = Builders<Product>.Filter.Where(x => x.Id.Equals(request.Id));
            var validProduct = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).AnyAsync();

            if (!validProduct)
            {
                throw new CustomException("Invalid product specified.");
            }

            var categoryFilter = Builders<Category>.Filter.Where(x => x.Id.Equals(request.CategoryId));
            var validCategory = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(categoryFilter).AnyAsync();

            if (!validCategory)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Product>()
                .Set(x => x.CategoryId, request.CategoryId)
                .Set(x => x.Name, request.Name.Trim())
                .Set(x => x.Description, request.Description.Trim())
                .Set(x => x.Stock, request.Stock)
                .Set(x => x.UnitPrice, request.UnitPrice)
                .Set(x => x.Base64Image, request.Base64Image);

            await _mongoDbService.GetCollection<Product>(Collections.Products)
                .UpdateOneAsync(session, productFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> ActivateOrDeactivateProduct(String id)
        {
            var productFilter = Builders<Product>.Filter.Where(x => x.Id.Equals(id));
            var product = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new CustomException("Invalid product specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Product>()
                .Set(x => x.Active, !product.Active);

            await _mongoDbService.GetCollection<Product>(Collections.Products)
                .UpdateOneAsync(session, productFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> DeleteProduct(String id)
        {
            var productFilter = Builders<Product>.Filter.Where(x => x.Id.Equals(id));
            var validProduct = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).AnyAsync();

            if (!validProduct)
            {
                throw new CustomException("Invalid product specified.");
            }

            var purchaseFilter = Builders<Purchase>.Filter.Where(x => x.Details.Any(d => d.ProductId.Equals(id)));
            var hasPurchases = await _mongoDbService.GetCollection<Purchase>(Collections.Purchases)
                .Find(purchaseFilter).AnyAsync();

            if (hasPurchases)
            {
                throw new CustomException("There are purchases related to this product.");
            }

            var saleFilter = Builders<Sale>.Filter.Where(x => x.Details.Any(d => d.ProductId.Equals(id)));
            var hasSales = await _mongoDbService.GetCollection<Sale>(Collections.Sales)
                .Find(saleFilter).AnyAsync();

            if (hasSales)
            {
                throw new CustomException("There are sales related to this product.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            await _mongoDbService.GetCollection<Product>(Collections.Products)
                .DeleteOneAsync(session, productFilter);

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
