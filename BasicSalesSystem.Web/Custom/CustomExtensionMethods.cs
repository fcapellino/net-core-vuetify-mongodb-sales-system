namespace BasicSalesSystem.Web.Custom
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text.RegularExpressions;

    public static class CustomExtensionMethods
    {
        public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, string propertyName)
        {
            if (!Regex.IsMatch(propertyName ?? string.Empty, "^[a-zA-Z]+(.[a-zA-Z]+)? (asc|desc)$"))
            {
                return query;
            }

            return query.OrderBy(propertyName);
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 5;
            }

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
