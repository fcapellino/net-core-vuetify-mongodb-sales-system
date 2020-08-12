namespace BasicSalesSystem.Web.Custom
{
    using System.Linq.Dynamic.Core;
    using System.Text.RegularExpressions;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    public static class CustomExtensionMethods
    {
        public static IFindFluent<T, T> ApplyOrdering<T>(this IFindFluent<T, T> query, string propertyName, bool descending)
        {
            if (!Regex.IsMatch(propertyName ?? string.Empty, "^[a-zA-Z]+(.[a-zA-Z]+)?$"))
            {
                return query;
            }

            var sortDefinition = descending ? Builders<T>.Sort.Descending(propertyName) : Builders<T>.Sort.Ascending(propertyName);
            return query.Sort(sortDefinition);
        }

        public static IMongoQueryable<T> ApplyOrdering<T>(this IMongoQueryable<T> query, string propertyName, bool descending)
        {
            if (!Regex.IsMatch(propertyName ?? string.Empty, "^[a-zA-Z]+(.[a-zA-Z]+)?$"))
            {
                return query;
            }

            return (IMongoQueryable<T>)query.OrderBy($"{propertyName} {(descending ? "DESC" : "ASC")}");
        }

        public static IFindFluent<T, T> ApplyPaging<T>(this IFindFluent<T, T> query, int page, int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 5;
            }

            return query.Skip((page - 1) * pageSize).Limit(pageSize);
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
