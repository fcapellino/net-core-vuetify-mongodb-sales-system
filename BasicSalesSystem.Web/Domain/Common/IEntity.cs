namespace BasicSalesSystem.Domain.Common
{
    using System;

    public interface IEntity
    {
        Guid Id { get; set; }
        bool IsDeleted { get; set; }
    }
}
