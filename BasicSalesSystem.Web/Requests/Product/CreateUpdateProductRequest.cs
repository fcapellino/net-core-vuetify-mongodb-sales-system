namespace BasicSalesSystem.Web.Requests.Product
{
    using System;
    using FluentValidation;

    public class CreateUpdateProductRequest
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
        public decimal UnitPrice { get; set; }
        public string Base64Image { get; set; }
    }

    public class CreateUpdateProductRequestValidator
        : AbstractValidator<CreateUpdateProductRequest>
    {
        public CreateUpdateProductRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0);

            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must(x =>
                {
                    var bytes = new Span<byte>(new byte[x.Length]);
                    return Convert.TryFromBase64String(x, bytes, out int bytesWritten);
                });
        }
    }
}
