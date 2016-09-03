using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    public class HomeRequest : IModelRequest<HomeViewModel>
    {
        [Range(1, Int32.MaxValue)]
        public Int32 Page { get; set; }

        public QuestionSorting? Sorting { get; set; }

        public HomeRequest()
        {
            Page = 1;
        }
    }
}
