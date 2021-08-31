namespace BasicSalesSystem.Web.Custom
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public sealed class SuccessResult : JsonResult
    {
        public SuccessResult(object resources)
            : base(new { Error = false, ErrorMessage = default(string), Exception = default(object), Resources = resources })
        {
        }

        public SuccessResult()
            : base(new { Error = false, ErrorMessage = default(string), Exception = default(object), Resources = default(object) })
        {
        }
    }

    public sealed class ErrorResult : JsonResult
    {
        public ErrorResult(string errorMessage, Exception exception)
            : base(new { Error = true, ErrorMessage = errorMessage, Exception = exception, Resources = default(object) })
        {
        }

        public ErrorResult(string errorMessage)
            : base(new { Error = true, ErrorMessage = errorMessage, Exception = default(object), Resources = default(object) })
        {
        }
    }
}
