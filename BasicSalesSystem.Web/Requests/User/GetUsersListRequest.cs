namespace BasicSalesSystem.Web.Requests.User
{
    using System;
    using FluentValidation;

    public class GetUsersListRequest
    {
        public string SearchQuery { get; set; }
        public string OrderByColumn { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Guid RoleId { get; set; }
    }

    public class GetUsersListRequestValidator
        : AbstractValidator<GetUsersListRequest>
    {
        public GetUsersListRequestValidator()
        {
        }
    }
}
