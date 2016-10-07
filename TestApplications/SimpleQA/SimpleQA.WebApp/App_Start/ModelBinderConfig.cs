using SimpleQA.WebApp.ModelBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp
{
    public static class ModelBinderConfig
    {
        public static void Configure(ModelBinderProviderCollection binders)
        {
            binders.Add(new QuestionModelBinderProvider());
            binders.Add(new CustomCancellationTokenModelBinder());
        }
    }
}