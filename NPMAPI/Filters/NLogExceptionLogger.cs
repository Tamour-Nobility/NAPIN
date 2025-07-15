using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using NPMAPI.Services;

namespace NPMAPI.Filters
{
    public class NLogExceptionLogger : ExceptionFilterAttribute, IExceptionFilter
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;
            String message = String.Empty;
            var exceptionType = actionExecutedContext.Exception.GetType();
            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                message = "Access to the Web API is not authorized.";
                status = HttpStatusCode.Unauthorized;
            }
            else
            {
                message = "Internal Server Error.";
                status = HttpStatusCode.InternalServerError;
            }
            actionExecutedContext.Response = new HttpResponseMessage()
            {
                Content = new StringContent(message, System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = status
            };
            NPMLogger.GetInstance().Error($"Message : {actionExecutedContext.Exception.Message}{Environment.NewLine}{Environment.NewLine}Inner Exception: {actionExecutedContext.Exception.InnerException}{Environment.NewLine}{Environment.NewLine}Stack Trace: {actionExecutedContext.Exception.StackTrace}");
            base.OnException(actionExecutedContext);
        }
    }
}