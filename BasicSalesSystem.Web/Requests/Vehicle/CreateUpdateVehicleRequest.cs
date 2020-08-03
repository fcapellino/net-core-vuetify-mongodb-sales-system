namespace BasicSalesSystem.Web.Requests.Vehicle
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;

    public class CreateUpdateVehicleRequest
    {
        public Guid Id { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public Guid ModelId { get; set; }
        public IList<Guid> FeaturesIds { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class CreateUpdateVehicleRequestValidator
        : AbstractValidator<CreateUpdateVehicleRequest>
    {
        public CreateUpdateVehicleRequestValidator()
        {
            RuleFor(x => x.ContactName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.ContactPhone)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ModelId)
                .NotEmpty();

            RuleFor(x => x)
                .NotNull();

            RuleFor(x => x.IsRegistered)
                .NotNull();
        }
    }
}
