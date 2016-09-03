using System;
using System.Collections.Generic;

namespace SimpleQA.Models
{
    public class HomeByTagViewModel : IPaginable, IOrderable
    {
        public Int32 Page { get; set; }
        public Int32 TotalPages { get; set; }
        public QuestionSorting Sorting { get; set; }
        public String Tag { get; set; }
        public IList<QuestionExcerptViewModel> Questions { get; set; }
    }
}