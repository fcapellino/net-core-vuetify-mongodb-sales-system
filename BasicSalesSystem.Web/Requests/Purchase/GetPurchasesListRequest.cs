namespace BasicSalesSystem.Web.Requests.Purchase
{
    using System;
    using FluentValidation;

    public class GetPurchasesListRequest
    {
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SupplierId { get; set; }
    }

    public class GetPurchasesListRequestValidator
        : AbstractValidator<GetPurchasesListRequest>
    {
        public GetPurchasesListRequestValidator()
        {
        }
    }
}
