namespace BasicSalesSystem.Web.Requests.User
{
    using FluentValidation;

    public class RefreshJSONWebTokenRequest
    {
        public string Token { get; set; }
    }

    public class RefreshJSONWebTokenRequestValidator
    : AbstractValidator<RefreshJSONWebTokenRequest>
    {
        public RefreshJSONWebTokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}
