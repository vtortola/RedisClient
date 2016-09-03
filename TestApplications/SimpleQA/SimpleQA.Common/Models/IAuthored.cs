using System;

namespace SimpleQA.Models
{
    public interface IAuthored
    {
        String User { get; }
        DateTime CreatedOn { get; }
    }
}