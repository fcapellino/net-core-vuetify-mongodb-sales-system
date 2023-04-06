namespace BasicSalesSystem.Web.Custom
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;

    public static class CustomMiddlewares
    {
        #region EXCEPTION HANDLER MIDDLEWARE
        public static void UseExceptionHandlerMiddleware(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(
                options =>
                {
                    options.Run(
                        async context =>
                        {
                            var ex = context.Features.Get<IExceptionHandlerFeature>();
                            var exception = ex.Error.GetBaseException();
                            var errorMessage = (exception is CustomException)
                                ? $"Error. {exception.Message}"
                                : $"Error. The operation cannot be completed.";

                            if (context.Request.Path.StartsWithSegments("/Api", StringComparison.OrdinalIgnoreCase))
                            {
                                var result = new ErrorResult(errorMessage, exception.GetBaseException());
                                var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
                                context.Response.StatusCode = StatusCodes.Status200OK;
                                await result.ExecuteResultAsync(actionContext);
                            }
                            else
                            {
                                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                await context.Response.CompleteAsync();
                            }
                        });
                }
            );
        }
        #endregion
    }
}
