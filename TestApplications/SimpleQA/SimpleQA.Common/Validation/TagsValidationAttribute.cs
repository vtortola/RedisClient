using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SimpleQA.Validation
{
    public class TagsValidationAttribute : ValidationAttribute
    {
        readonly Int32 _min, _max;

        public TagsValidationAttribute(Int32 min, Int32 max)
        {
            _min = min;
            _max = max;
        }

        protected override ValidationResult IsValid(Object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var svalue = value as String[];
                if (svalue == null)
                    return new ValidationResult("String expected.");

                if (svalue.Length > _max)
                    return new ValidationResult("Only " + _max + " tags are allowed.");

                if (svalue.Length < _min)
                    return new ValidationResult("Minimum " + _min + " tag is required.");
            }

            return null;
        }
    }
}