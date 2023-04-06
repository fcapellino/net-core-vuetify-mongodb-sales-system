namespace BasicSalesSystem.Web.Requests.Customer
{
    using System.Linq;
    using BasicSalesSystem.Web.Custom.Enumerations;
    using FluentValidation;

    public class CreateUpdateCustomerRequest
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DocumentType { get; set; }
        public long DocumentNumber { get; set; }
    }

    public class CreateUpdateCustomerRequestValidator
        : AbstractValidator<CreateUpdateCustomerRequest>
    {
        public CreateUpdateCustomerRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(40);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.PhoneNumber)
               .NotEmpty()
               .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.DocumentType)
                .NotEmpty()
                .Must(x =>
                {
                    return new[] { DocumentTypes.DNI, DocumentTypes.PASSPORT }.Contains(x.ToLower());
                });

            RuleFor(x => x.DocumentNumber)
                .GreaterThan(0);
        }
    }
}
