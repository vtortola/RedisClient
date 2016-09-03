using System;

namespace SimpleQA.Models
{
    public interface IVotable
    {
        String Id { get; }
        Int64 Score { get; }
        Int32 UpVotes { get; }
        Int32 DownVotes { get; }
        Boolean? UpVoted { get; }
        Boolean AuthoredByUser { get; }
        Boolean Votable { get; }
    }
}