using System;

namespace SimpleQA.Models
{
    public sealed class PopularTagsRequest : IModelRequest<PopularTagsViewModel>
    {
    }

    public sealed class PopularTagsViewModel : IModel
    {
        public String[] Tags { get; set; }
    }
}
