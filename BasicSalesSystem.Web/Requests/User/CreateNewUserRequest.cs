namespace BasicSalesSystem.Web.Requests.User
{
    using System.Linq;
    using BasicSalesSystem.Web.Custom.Enumerations;
    using FluentValidation;

    public class CreateNewUserRequest
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string DocumentType { get; set; }
        public int DocumentNumber { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class CreateNewUserRequestValidator
        : AbstractValidator<CreateNewUserRequest>
    {
        public CreateNewUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(40);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.DocumentType)
                .NotEmpty()
                .Must(x =>
                {
                    return new[] { DocumentTypes.DNI, DocumentTypes.PASSPORT }.Contains(x.ToLower());
                });

            RuleFor(x => x.DocumentNumber)
                .GreaterThan(0);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(x =>
                {
                    return new[] { UserRoles.Administrator, UserRoles.Salesman, UserRoles.Storekeeper }.Contains(x.ToLower());
                });
        }
    }
}
