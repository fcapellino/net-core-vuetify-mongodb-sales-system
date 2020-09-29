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
    using BasicSalesSystem.Web.Requests.Supplier;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class SuppliersController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public SuppliersController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> GetSuppliersList(GetSuppliersListRequest request)
        {
            var query = _mongoDbService.GetCollection<Supplier>(Collections.Suppliers).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                long.TryParse(request.SearchQuery, out long documentNumber);

                query = query.Where(x => x.FullName.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                                         x.Address.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                                         x.Email.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                                         x.DocumentNumber == documentNumber);
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
                        return new
                        {
                            item.Id,
                            item.FullName,
                            item.Address,
                            item.PhoneNumber,
                            item.Email,
                            item.DocumentType,
                            item.DocumentNumber,
                            item.Active
                        };
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateUpdateSupplierRequest request)
        {
            var duplicateSupplierFilter = Builders<Supplier>.Filter.Where(x =>
                x.Email.Equals(request.Email.Trim().ToLower()) ||
                (x.DocumentType.Equals(request.DocumentType.Trim().ToLower()) && x.DocumentNumber.Equals(request.DocumentNumber)));

            var supplierAlreadyExists = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(duplicateSupplierFilter).AnyAsync();

            if (supplierAlreadyExists)
            {
                throw new CustomException("Duplicate email or document number.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var newSupplier = new Supplier()
            {
                FullName = request.FullName.Trim(),
                Address = request.Address.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email.Trim().Normalize().ToLowerInvariant(),
                DocumentType = request.DocumentType.Trim().ToLowerInvariant(),
                DocumentNumber = request.DocumentNumber,
                Active = true
            };

            await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers).InsertOneAsync(session, newSupplier);
            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> UpdateSupplier([FromBody] CreateUpdateSupplierRequest request)
        {
            var supplierFilter = Builders<Supplier>.Filter.Where(x => x.Id.Equals(request.Id));
            var validSupplier = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(supplierFilter).AnyAsync();

            if (!validSupplier)
            {
                throw new CustomException("Invalid Supplier specified.");
            }

            var duplicateSupplierFilter = Builders<Supplier>.Filter.Where(x =>
                !x.Id.Equals(request.Id) &&
                (x.Email.Equals(request.Email.Trim().ToLower()) ||
                (x.DocumentType.Equals(request.DocumentType.Trim().ToLower()) && x.DocumentNumber.Equals(request.DocumentNumber))));

            var supplierAlreadyExists = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(duplicateSupplierFilter).AnyAsync();

            if (supplierAlreadyExists)
            {
                throw new CustomException("Duplicate email or document number.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Supplier>()
                .Set(x => x.FullName, request.FullName.Trim())
                .Set(x => x.Address, request.Address.Trim())
                .Set(x => x.PhoneNumber, request.PhoneNumber.Trim())
                .Set(x => x.Email, request.Email.Trim().Normalize().ToLowerInvariant())
                .Set(x => x.DocumentType, request.DocumentType.Trim().ToLowerInvariant())
                .Set(x => x.DocumentNumber, request.DocumentNumber);

            await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .UpdateOneAsync(session, supplierFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> ActivateOrDeactivateSupplier(String id)
        {
            var supplierFilter = Builders<Supplier>.Filter.Where(x => x.Id.Equals(id));
            var supplier = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(supplierFilter).FirstOrDefaultAsync();

            if (supplier == null)
            {
                throw new CustomException("Invalid Supplier specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Supplier>()
                .Set(x => x.Active, !supplier.Active);

            await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .UpdateOneAsync(session, supplierFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Storekeeper)]
        public async Task<IActionResult> DeleteSupplier(String id)
        {
            var supplierFilter = Builders<Supplier>.Filter.Where(x => x.Id.Equals(id));
            var validSupplier = await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .Find(supplierFilter).AnyAsync();

            if (!validSupplier)
            {
                throw new CustomException("Invalid Supplier specified.");
            }

            var purchaseFilter = Builders<Purchase>.Filter.Where(x => x.SupplierId.Equals(id));
            var hasPurchases = await _mongoDbService.GetCollection<Purchase>(Collections.Purchases)
                .Find(purchaseFilter).AnyAsync();

            if (hasPurchases)
            {
                throw new CustomException("There are purchases related to this supplier.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            await _mongoDbService.GetCollection<Supplier>(Collections.Suppliers)
                .DeleteOneAsync(session, supplierFilter);

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
