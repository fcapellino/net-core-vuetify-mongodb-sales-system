namespace BasicSalesSystem.Web.Requests.Customer
{
    using FluentValidation;

    public class GetCustomersListRequest
    {
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetCustomersListRequestValidator
        : AbstractValidator<GetCustomersListRequest>
    {
        public GetCustomersListRequestValidator()
        {
        }
    }
}
