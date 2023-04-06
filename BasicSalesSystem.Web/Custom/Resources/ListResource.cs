namespace BasicSalesSystem.Web.Custom.Resources
{
    using System.Collections.Generic;

    public class ListResource
    {
        public IEnumerable<object> ItemsList { get; set; }

        public ListResource()
        {
        }
        public ListResource(IEnumerable<object> itemsList)
            : this()
        {
            ItemsList = itemsList;
        }
    }
}
