using RabbitMQ.Client;

namespace Application.Interfaces
{
    public interface IRabbitMqConnectionManager : IDisposable
    {
        IConnection GetConnection();
        IChannel GetChannel();
        Task InitializeAsync();
    }
}
