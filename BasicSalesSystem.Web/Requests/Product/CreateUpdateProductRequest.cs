namespace BasicSalesSystem.Web.Requests.Product
{
    using FluentValidation;

    public class CreateUpdateProductRequest
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string BarCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateUpdateProductRequestValidator
        : AbstractValidator<CreateUpdateProductRequest>
    {
        public CreateUpdateProductRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty();

            RuleFor(x => x.BarCode)
                .NotEmpty()
                .Length(13);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.Stock)
                .GreaterThan(0);

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0);
        }
    }
}
