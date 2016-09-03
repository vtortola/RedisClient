using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestion))]
    public class QuestionEditFormViewModel : IQuestion, IModel
    {
        public String Id { get; set; }

        public String Title { get; set; }

        public String Content { get; set; }

        public String[] Tags { get; set; }
    }
}