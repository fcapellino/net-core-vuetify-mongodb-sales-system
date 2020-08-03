namespace BasicSalesSystem.Web.Requests.Vehicle
{
    using System;
    using FluentValidation;

    public class GetVehiclesListRequest
    {
        public string SearchQuery { get; set; }
        public string OrderByColumn { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Guid MakeId { get; set; }
        public Guid ModelId { get; set; }
    }

    public class GetVehiclesListRequestValidator
        : AbstractValidator<GetVehiclesListRequest>
    {
        public GetVehiclesListRequestValidator()
        {
        }
    }
}
