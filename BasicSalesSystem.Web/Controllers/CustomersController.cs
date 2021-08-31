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
    using BasicSalesSystem.Web.Requests.Customer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    [Authorize]
    [Route("Api/[controller]/[action]")]
    public class CustomersController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public CustomersController(IServiceProvider serviceProvider)
        {
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
        }

        [HttpGet]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> GetCustomersList(GetCustomersListRequest request)
        {
            var query = _mongoDbService.GetCollection<Customer>(Collections.Customers).AsQueryable();

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
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateUpdateCustomerRequest request)
        {
            var duplicateCustomerFilter = Builders<Customer>.Filter.Where(x =>
                x.Email.Equals(request.Email.Trim().ToLower()) ||
                (x.DocumentType.Equals(request.DocumentType.Trim().ToLower()) && x.DocumentNumber.Equals(request.DocumentNumber)));

            var customerAlreadyExists = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(duplicateCustomerFilter).AnyAsync();

            if (customerAlreadyExists)
            {
                throw new CustomException("Duplicate email or document number.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var newCustomer = new Customer()
            {
                FullName = request.FullName.Trim(),
                Address = request.Address.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email.Trim().Normalize().ToLowerInvariant(),
                DocumentType = request.DocumentType.Trim().ToLowerInvariant(),
                DocumentNumber = request.DocumentNumber,
                Active = true
            };

            await _mongoDbService.GetCollection<Customer>(Collections.Customers).InsertOneAsync(session, newCustomer);
            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> UpdateCustomer([FromBody] CreateUpdateCustomerRequest request)
        {
            var customerFilter = Builders<Customer>.Filter.Where(x => x.Id.Equals(request.Id));
            var validCustomer = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(customerFilter).AnyAsync();

            if (!validCustomer)
            {
                throw new CustomException("Invalid customer specified.");
            }

            var duplicateCustomerFilter = Builders<Customer>.Filter.Where(x =>
                !x.Id.Equals(request.Id) &&
                (x.Email.Equals(request.Email.Trim().ToLower()) ||
                (x.DocumentType.Equals(request.DocumentType.Trim().ToLower()) && x.DocumentNumber.Equals(request.DocumentNumber))));

            var customerAlreadyExists = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(duplicateCustomerFilter).AnyAsync();

            if (customerAlreadyExists)
            {
                throw new CustomException("Duplicate email or document number.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Customer>()
                .Set(x => x.FullName, request.FullName.Trim())
                .Set(x => x.Address, request.Address.Trim())
                .Set(x => x.PhoneNumber, request.PhoneNumber.Trim())
                .Set(x => x.Email, request.Email.Trim().Normalize().ToLowerInvariant())
                .Set(x => x.DocumentType, request.DocumentType.Trim().ToLowerInvariant())
                .Set(x => x.DocumentNumber, request.DocumentNumber);

            await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .UpdateOneAsync(session, customerFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> ActivateOrDeactivateCustomer(String id)
        {
            var customerFilter = Builders<Customer>.Filter.Where(x => x.Id.Equals(id));
            var customer = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(customerFilter).FirstOrDefaultAsync();

            if (customer == null)
            {
                throw new CustomException("Invalid customer specified.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            var updateDefinition = new UpdateDefinitionBuilder<Customer>()
                .Set(x => x.Active, !customer.Active);

            await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .UpdateOneAsync(session, customerFilter, updateDefinition);

            await _mongoDbService.CommitTransactionAsync();
            return new SuccessResult();
        }

        [HttpPost]
        [Authorization(UserRoles.Administrator, UserRoles.Salesman)]
        public async Task<IActionResult> DeleteCustomer(String id)
        {
            var customerFilter = Builders<Customer>.Filter.Where(x => x.Id.Equals(id));
            var validCustomer = await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .Find(customerFilter).AnyAsync();

            if (!validCustomer)
            {
                throw new CustomException("Invalid customer specified.");
            }

            var saleFilter = Builders<Sale>.Filter.Where(x => x.CustomerId.Equals(id));
            var hasSales = await _mongoDbService.GetCollection<Sale>(Collections.Sales)
                .Find(saleFilter).AnyAsync();

            if (hasSales)
            {
                throw new CustomException("There are sales related to this customer.");
            }

            var session = await _mongoDbService.StartTransactionAsync();
            await _mongoDbService.GetCollection<Customer>(Collections.Customers)
                .DeleteOneAsync(session, customerFilter);

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
