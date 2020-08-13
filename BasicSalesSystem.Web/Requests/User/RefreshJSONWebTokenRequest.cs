namespace BasicSalesSystem.Web.Requests.User
{
    using FluentValidation;

    public class RefreshJSONWebTokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshJSONWebTokenRequestValidator
    : AbstractValidator<RefreshJSONWebTokenRequest>
    {
        public RefreshJSONWebTokenRequestValidator()
        {

        }
    }
}
