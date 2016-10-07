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
            var cancelSource = new CancellationTokenSource();
            controllerContext.HttpContext.Response.ClientDisconnectedToken.Register(cancelSource.Cancel);
            controllerContext.HttpContext.Request.TimedOutToken.Register(cancelSource.Cancel);
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