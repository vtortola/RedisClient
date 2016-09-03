using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    public interface IPaginable : IModel
    {
        Int32 Page { get; }
        Int32 TotalPages { get;}
    }
}
