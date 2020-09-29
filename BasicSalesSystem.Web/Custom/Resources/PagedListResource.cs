namespace BasicSalesSystem.Web.Custom.Resources
{
    using System.Collections.Generic;

    public sealed class PagedListResource
    {
        public IEnumerable<object> ItemsList { get; set; }
        public long TotalItemCount { get; set; }

        public PagedListResource()
        {
        }
        public PagedListResource(IEnumerable<object> itemsList, long totalItemCount)
            : this()
        {
            ItemsList = itemsList;
            TotalItemCount = totalItemCount;
        }
    }
}
