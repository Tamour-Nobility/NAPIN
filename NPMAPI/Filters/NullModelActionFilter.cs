﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace NPMAPI.Filters
{
    public class NullModelStateActionFilter : ActionFilterAttribute
    {
        public bool ReturnsBadRequest { get; set; } = false;
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ActionDescriptor.GetCustomAttributes<NullableModelAttribute>().Any() && actionContext.ActionArguments.ContainsValue(null))
            {
                actionContext.ModelState.AddModelError("Error", "Null Model Not Allowed");

                if (ReturnsBadRequest)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse
                        (HttpStatusCode.BadRequest, actionContext.ModelState);
                }
            }
        }
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class NullableModelAttribute : Attribute
    {
    }
}