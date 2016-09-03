using System;

namespace SimpleQA.Models
{
    public class QuestionAskFormRequest : IModelRequest<QuestionAddFormViewModel>
    {
        public String Tag { get; set; }
    }
}