namespace Application.Interfaces
{
    public interface IRabbitMqConsumer
    {
        Task StartConsumingAsync(CancellationToken stoppingToken);
    }
}
