using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    public interface IOrderable : IModel
    {
        QuestionSorting Sorting { get; }
    }
}
