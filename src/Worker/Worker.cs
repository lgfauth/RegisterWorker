using Application.Interfaces;
using Domain.Models;
using MicroservicesLogger.Interfaces;

namespace RegisterWorker
{
    public class Worker : BackgroundService
    {
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUnsubscriptionService _unsubscriptionService;

        public Worker(IWorkerLog<WorkerLogModel> logger, ISubscriptionService subscriptionService, IUnsubscriptionService unsubscriptionService)
        {
            _logger = logger;
            _subscriptionService = subscriptionService;
            _unsubscriptionService = unsubscriptionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Worker running at: {DateTimeOffset.Now}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
