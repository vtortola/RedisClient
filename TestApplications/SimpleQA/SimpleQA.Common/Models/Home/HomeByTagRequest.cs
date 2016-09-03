using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public class HomeByTagRequest : IModelRequest<HomeByTagViewModel>
    {
        [Required]
        public String Tag { get; set; }

        [Range(1, Int32.MaxValue)]
        public Int32 Page { get; set; }

        public QuestionSorting? Sorting { get; set; }

        public HomeByTagRequest()
        {
            Page = 1;
        }
    }
}
