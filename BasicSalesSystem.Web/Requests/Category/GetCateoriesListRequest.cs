namespace BasicSalesSystem.Web.Requests.Category
{
    using FluentValidation;

    public class GetCateoriesListRequest
    {
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetCateoriesListRequestValidator
        : AbstractValidator<GetCateoriesListRequest>
    {
        public GetCateoriesListRequestValidator()
        {
        }
    }
}
