namespace BasicSalesSystem.Web.Requests.Category
{
    using FluentValidation;

    public class CreateUpdateCategoryRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateUpdateCategoryRequestValidator
        : AbstractValidator<CreateUpdateCategoryRequest>
    {
        public CreateUpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(300);
        }
    }
}
