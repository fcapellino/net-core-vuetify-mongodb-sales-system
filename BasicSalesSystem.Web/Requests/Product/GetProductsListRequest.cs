namespace BasicSalesSystem.Web.Requests.Product
{
    using FluentValidation;

    public class GetProductsListRequest
    {
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string CategoryId { get; set; }
    }

    public class GetProductsListRequestValidator
        : AbstractValidator<GetProductsListRequest>
    {
        public GetProductsListRequestValidator()
        {
        }
    }
}
