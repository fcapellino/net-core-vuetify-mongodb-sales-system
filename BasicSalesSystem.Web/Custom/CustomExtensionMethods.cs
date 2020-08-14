namespace BasicSalesSystem.Web.Custom
{
    using System.Linq.Dynamic.Core;
    using System.Text.RegularExpressions;
    using MongoDB.Driver.Linq;

    public static class CustomExtensionMethods
    {
        public static IMongoQueryable<T> ApplyOrdering<T>(this IMongoQueryable<T> query, string propertyName, bool descending)
        {
            if (!Regex.IsMatch(propertyName ?? string.Empty, "^[a-zA-Z]+(.[a-zA-Z]+)?$"))
            {
                return query;
            }

            return (IMongoQueryable<T>)query.OrderBy($"{propertyName} {(descending ? "DESC" : "ASC")}");
        }

        public static IMongoQueryable<T> ApplyPaging<T>(this IMongoQueryable<T> query, int page, int pageSize)
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
