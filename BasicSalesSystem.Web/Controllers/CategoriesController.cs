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
    using BasicSalesSystem.Web.Requests.Category;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class CategoriesController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public CategoriesController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetCategoriesList(GetCategoriesListRequest request)
        {
            var query = _mongoDbService.GetCollection<Category>(Collections.Categories).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                                         x.Description.ToLower().Contains(request.SearchQuery.ToLower().Trim()));
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
                    .Select(item => new
                    {
                        item.Id,
                        item.Name,
                        item.Description,
                        item.Active
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetCompleteCategoriesList()
        {
            var query = _mongoDbService.GetCollection<Category>(Collections.Categories).AsQueryable();
            var items = await query
                .ApplyOrdering(nameof(Category.Name), descending: false)
                .ToListAsync();

            var resources = new ListResource()
            {
                ItemsList = items
                    .Select(item => new
                    {
                        item.Id,
                        item.Name,
                        item.Description
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateUpdateCategoryRequest request)
        {
            var session = await _mongoDbService.StartTransactionAsync();
            var newCategory = new Category()
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Active = true
            };

            await _mongoDbService.GetCollection<Category>(Collections.Categories).InsertOneAsync(session, newCategory);
            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> UpdateCategory([FromBody] CreateUpdateCategoryRequest request)
        {
            var categoryFilter = Builders<Category>.Filter.Where(x => x.Id.Equals(request.Id));
            var validCategory = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(categoryFilter).AnyAsync();

            if (!validCategory)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Category>()
                .Set(x => x.Name, request.Name.Trim())
                .Set(x => x.Description, request.Description.Trim());

            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .UpdateOneAsync(session, categoryFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> ActivateOrDeactivateCategory(String id)
        {
            var categoryFilter = Builders<Category>.Filter.Where(x => x.Id.Equals(id));
            var category = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(categoryFilter).FirstOrDefaultAsync();

            if (category == null)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Category>()
                .Set(x => x.Active, !category.Active);

            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .UpdateOneAsync(session, categoryFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> DeleteCategory(String id)
        {
            var categoryFilter = Builders<Category>.Filter.Where(x => x.Id.Equals(id));
            var validCategory = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(categoryFilter).AnyAsync();

            if (!validCategory)
            {
                throw new CustomException("Invalid category specified.");
            }

            var productFilter = Builders<Product>.Filter.Where(x => x.CategoryId.Equals(id));
            var hasProducts = await _mongoDbService.GetCollection<Product>(Collections.Products)
                .Find(productFilter).AnyAsync();

            if (hasProducts)
            {
                throw new CustomException("There are products assigned to this category.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .DeleteOneAsync(session, categoryFilter);

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
