namespace BasicSalesSystem.Web.Custom
{
    using Microsoft.AspNetCore.Mvc.Filters;

    public class CustomModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                throw new CustomException("The received data model is invalid.");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // method intentionally left empty.
        }
    }
}
