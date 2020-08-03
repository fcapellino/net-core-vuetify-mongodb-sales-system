namespace BasicSalesSystem.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Identity;

    [Table("AspNetRoles")]
    public class ApplicationRole : IdentityRole<Guid>
    {
        [InverseProperty("Role")]
        public IList<ApplicationUserRole> UserRoles { get; set; }
    }
}
