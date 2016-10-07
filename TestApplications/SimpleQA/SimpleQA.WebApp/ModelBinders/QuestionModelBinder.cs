using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.ModelBinders
{
    public class QuestionModelBinderProvider : IModelBinderProvider
    {
        readonly QuestionModelBinder _binder;
        public QuestionModelBinderProvider()
        {
            _binder = new QuestionModelBinder();
        }
        public IModelBinder GetBinder(Type modelType)
        {
            if (typeof(IQuestionData).IsAssignableFrom(modelType))
                return _binder;
            return null;
        }
    }

    public class QuestionModelBinder : DefaultModelBinder
    {
        static readonly String[] _empty = new String[0];
        static readonly Char[] _tagSplit = new Char[] { ',' };

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.Name.Equals("tags", StringComparison.OrdinalIgnoreCase))
            {
                var tagsValues = bindingContext.ValueProvider.GetValue("tags").RawValue as String[];
                var model = (IQuestionData)bindingContext.Model;
                if(tagsValues != null && tagsValues.Any())
                {
                    model.Tags = tagsValues[0].ToString().Split(_tagSplit, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < model.Tags.Length; i++)
                    {
                        model.Tags[i] = model.Tags[i].Replace(' ', '-');
                    }
                }
                else
                {
                    model.Tags = _empty;
                }
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }
    }

}