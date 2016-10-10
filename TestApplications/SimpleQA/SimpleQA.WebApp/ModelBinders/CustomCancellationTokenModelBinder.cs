using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.ModelBinders
{
    public class CustomCancellationTokenModelBinder : IModelBinder, IModelBinderProvider
    {
        static readonly Type _type = typeof(CancellationToken);

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(
                                    controllerContext.HttpContext.Response.ClientDisconnectedToken,
                                    controllerContext.HttpContext.Request.TimedOutToken);
            return cancelSource.Token;
        }

        public IModelBinder GetBinder(Type modelType)
        {
            if (modelType == _type)
                return this;

            return null;
        }
    }
}