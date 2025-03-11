using Application.Interfaces;

namespace RegisterWorker
{
    public class Worker : BackgroundService
    {
        private readonly IRabbitMqConsumer _consumer;
        private readonly IRabbitMqConnectionManager _connectionManager;

        public Worker(IRabbitMqConsumer consumer, IRabbitMqConnectionManager connectionManager)
        {
            _consumer = consumer;
            _connectionManager = connectionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _connectionManager.InitializeAsync();
            //await _consumer.StartConsumingAsync(stoppingToken);

            Console.WriteLine("Worker está funcionando...");

            await Task.Delay(TimeSpan.FromSeconds(15).Milliseconds);
        }

        public override void Dispose()
        {
            _connectionManager.Dispose();
            base.Dispose();
        }
    }
}