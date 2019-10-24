using ASC.Business.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Utilities;

namespace ASC.Web.Filters
{
    /// <summary>
    /// Para tornar mais fácil de implementar um filtro de ação personalizada, a estrutura ASP.NET MVC inclui uma base de ActionFilterAttribute classe. 
    /// Essa classe implementa ambos os IActionFilter e IResultFilter interfaces e herda o Filter classe.
    /// </summary>
    public class UserActivityFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogDataOperations)) as ILogDataOperations;
            await logger.CreateUserActivityAsync(context.HttpContext.User.GetCurrentUserDetails().Email, context.HttpContext.Request.Path);

        }
    }
}
