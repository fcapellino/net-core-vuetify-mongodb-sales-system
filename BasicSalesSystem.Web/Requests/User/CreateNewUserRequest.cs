namespace BasicSalesSystem.Web.Requests.User
{
    using System.Linq;
    using FluentValidation;
    using BasicSalesSystem.Web.Custom.Enumerations;

    public class CreateNewUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class CreateNewUserRequestValidator
        : AbstractValidator<CreateNewUserRequest>
    {
        public CreateNewUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(x =>
                {
                    return new[] { UserRoles.Administrator, UserRoles.Regular }.Contains(x.ToLower());
                });
        }
    }
}
