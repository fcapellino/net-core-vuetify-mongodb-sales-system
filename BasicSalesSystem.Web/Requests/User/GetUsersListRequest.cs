namespace BasicSalesSystem.Web.Requests.User
{
    using FluentValidation;

    public class GetUsersListRequest
    {
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string RoleId { get; set; }
    }

    public class GetUsersListRequestValidator
        : AbstractValidator<GetUsersListRequest>
    {
        public GetUsersListRequestValidator()
        {
        }
    }
}
