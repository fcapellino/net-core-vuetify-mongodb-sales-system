namespace BasicSalesSystem.Web.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
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

        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            await Task.WhenAll(enumerable.Select(item => action(item)));
        }

        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
