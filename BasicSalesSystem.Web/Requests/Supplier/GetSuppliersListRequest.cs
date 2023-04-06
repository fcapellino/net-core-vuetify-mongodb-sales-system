namespace BasicSalesSystem.Web.Requests.Supplier
{
    using FluentValidation;

    public class GetSuppliersListRequest
    {
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetSuppliersListRequestValidator
        : AbstractValidator<GetSuppliersListRequest>
    {
        public GetSuppliersListRequestValidator()
        {
        }
    }
}
