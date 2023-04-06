namespace BasicSalesSystem.Web.Requests.User
{
    using FluentValidation;

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangePasswordRequestValidator
    : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotNull()
                .MinimumLength(8);

            RuleFor(x => x.NewPassword)
                .NotNull()
                .MinimumLength(8);
        }
    }
}
