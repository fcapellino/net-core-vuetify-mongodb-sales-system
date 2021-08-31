namespace BasicSalesSystem.Web.Custom.Enumerations
{
    public static class ExtraClaimTypes
    {
        public const string FullName = "fullname";
        public const string RefreshCode = "refreshcode";
    }

    public static class UserRoles
    {
        public const string Administrator = "administrator";
        public const string Salesman = "salesman";
        public const string Storekeeper = "storekeeper";
    }

    public static class DocumentTypes
    {
        public const string DNI = "dni";
        public const string PASSPORT = "passport";
    }

    public static class ReceiptTypes
    {
        public const string CERTIFICATE = "certificate";
        public const string INVOICE = "invoice";
        public const string TICKET = "ticket";
    }

    public static class Collections
    {
        public const string Migrations = "_migrations";
        public const string Users = "users";
        public const string Roles = "roles";

        public const string Categories = "categories";
        public const string Customers = "customers";
        public const string Products = "products";
        public const string Purchases = "purchases";
        public const string Sales = "sales";
        public const string Suppliers = "suppliers";
    }
}
