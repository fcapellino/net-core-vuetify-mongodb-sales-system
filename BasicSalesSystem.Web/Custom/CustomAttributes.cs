namespace BasicSalesSystem.Web.Custom
{
    using Microsoft.AspNetCore.Authorization;

    internal class AuthorizationAttribute : AuthorizeAttribute
    {
        public AuthorizationAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
