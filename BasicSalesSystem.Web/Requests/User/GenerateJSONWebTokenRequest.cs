namespace BasicSalesSystem.Web.Requests.User
{
    using FluentValidation;

    public class GenerateJSONWebTokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GenerateJSONWebTokenRequestValidator
    : AbstractValidator<GenerateJSONWebTokenRequest>
    {
        public GenerateJSONWebTokenRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
