namespace BasicSalesSystem.Web.Requests.Purchase
{
    using System.Collections.Generic;
    using System.Linq;
    using BasicSalesSystem.Web.Custom.Enumerations;
    using FluentValidation;

    public class CreatePurchaseRequest
    {
        public string SupplierId { get; set; }
        public string ReceiptType { get; set; }
        public decimal Tax { get; set; }
        public IList<CreatePurchaseDetailRequest> Details { get; set; }
    }

    public class CreatePurchaseDetailRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreatePurchaseRequestValidator
        : AbstractValidator<CreatePurchaseRequest>
    {
        public CreatePurchaseRequestValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty();

            RuleFor(x => x.ReceiptType)
                .NotEmpty()
                .Must(x =>
                {
                    return new[] { ReceiptTypes.CERTIFICATE, ReceiptTypes.INVOICE, ReceiptTypes.TICKET }.Contains(x.ToLower());
                });

            RuleFor(x => x.Tax)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Details)
                .NotNull()
                .Must(d => d.Count > 0 && !d.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
                .ForEach(rule => rule.ChildRules(items =>
                {
                    items.RuleFor(x => x.ProductId)
                        .NotEmpty();

                    items.RuleFor(x => x.Quantity)
                        .GreaterThan(0);

                    items.RuleFor(x => x.UnitPrice)
                        .GreaterThan(0);
                }));
        }
    }
}
