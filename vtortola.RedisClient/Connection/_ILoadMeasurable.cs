using System;

namespace vtortola.Redis
{
    internal interface ILoadMeasurable
    {
        Int32 CurrentLoad { get; }
    }
}
