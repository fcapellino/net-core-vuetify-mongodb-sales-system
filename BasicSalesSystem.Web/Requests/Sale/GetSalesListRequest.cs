namespace BasicSalesSystem.Web.Requests.Sale
{
    using System;
    using FluentValidation;

    public class GetSalesListRequest
    {
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CustomerId { get; set; }
    }

    public class GetSalesListRequestValidator
        : AbstractValidator<GetSalesListRequest>
    {
        public GetSalesListRequestValidator()
        {
        }
    }
}
