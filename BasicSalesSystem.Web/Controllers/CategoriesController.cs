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

    //[Authorize]
    [AllowAnonymous]
    [Route("Api/[controller]/[action]")]
    public class CategoriesController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public CategoriesController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> GetCategory(String id)
        {
            var filter = Builders<Category>.Filter.Where(x => x.Id.Equals(id));
            var category = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(filter).FirstOrDefaultAsync();

            if (category == null)
            {
                throw new CustomException("Invalid category specified.");
            }

            var item = new
            {
                category.Id,
                category.Name,
                category.Description,
                category.Active,
            };

            return new SuccessResult(item);
        }

        [HttpGet]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> GetCategoriesList(GetCategoriesListRequest request)
        {
            var query = _mongoDbService.GetCollection<Category>(Collections.Categories);
            var filter = new FilterDefinitionBuilder<Category>().Empty;
            var findOptions = new FindOptions() { Collation = new Collation("en") };

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                filter &= Builders<Category>.Filter.Or(
                    Builders<Category>.Filter.Where(x => x.Name.ToLower().Contains(request.SearchQuery.ToLower().Trim())),
                    Builders<Category>.Filter.Where(x => x.Description.ToLower().Contains(request.SearchQuery.ToLower().Trim())));
            }

            var totalItemCount = await query.Find(filter).CountDocumentsAsync();
            var items = await query.Find(filter, findOptions)
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

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateUpdateCategoryRequest request)
        {
            var session = await _mongoDbService.GetSessionAsync();
            await _mongoDbService.StartTransactionAsync();

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
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> UpdateCategory([FromBody] CreateUpdateCategoryRequest request)
        {
            var filter = Builders<Category>.Filter.Where(x => x.Id.Equals(request.Id));
            var category = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(filter).FirstOrDefaultAsync();

            if (category == null)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.GetSessionAsync();
            await _mongoDbService.StartTransactionAsync();

            var updateDefinition = new UpdateDefinitionBuilder<Category>()
                .Set(x => x.Name, request.Name.Trim())
                .Set(x => x.Description, request.Description.Trim());

            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                 .UpdateOneAsync(session, filter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> ActivateOrDeactivateCategory(String id)
        {
            var filter = Builders<Category>.Filter.Where(x => x.Id.Equals(id));
            var category = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(filter).FirstOrDefaultAsync();

            if (category == null)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.GetSessionAsync();
            await _mongoDbService.StartTransactionAsync();

            var updateDefinition = new UpdateDefinitionBuilder<Category>()
                .Set(x => x.Active, !category.Active);

            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                 .UpdateOneAsync(session, filter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> DeleteCategory(String id)
        {
            var filter = Builders<Category>.Filter.Where(x => x.Id.Equals(id));
            var category = await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .Find(filter).FirstOrDefaultAsync();

            if (category == null)
            {
                throw new CustomException("Invalid category specified.");
            }

            var session = await _mongoDbService.GetSessionAsync();
            await _mongoDbService.StartTransactionAsync();
            await _mongoDbService.GetCollection<Category>(Collections.Categories)
                .DeleteOneAsync(session, filter);

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
