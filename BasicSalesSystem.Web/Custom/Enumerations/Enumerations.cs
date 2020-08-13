namespace BasicSalesSystem.Web.Custom.Enumerations
{
    public static class UserRoles
    {
        public const string Administrator = "administrator";
        public const string Salesman = "salesman";
        public const string Storekeeper = "storekeeper";
    }

    public static class DocumentTypes
    {
        public const string CI = "ci";
        public const string DNI = "dni";
        public const string LC = "lc";
        public const string LE = "le";
    }

    public static class Collections
    {
        public const string Categories = "categories";
        public const string Products = "products";

        public const string Roles = "roles";
        public const string Users = "users";
    }
}
