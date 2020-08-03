namespace BasicSalesSystem.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Identity;

    [Table("AspNetUserRoles")]
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        [Required]
        public ApplicationUser User { get; set; }

        [Required]
        public ApplicationRole Role { get; set; }
    }
}
