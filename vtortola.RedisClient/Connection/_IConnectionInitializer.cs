
namespace vtortola.Redis
{
    internal interface IConnectionInitializer
    {
        void Initialize(SocketReader reader, SocketWriter writer);
    }
}
