using System;
using System.Collections.Generic;

namespace SimpleQA.Models
{
    public class HomeViewModel : IPaginable, IOrderable
    {
        public Int32 Page { get; set; }
        public Int32 TotalPages { get; set; }
        public QuestionSorting Sorting { get; set; }
        public IList<QuestionHeaderViewModel> Questions { get; set; }
    }
}