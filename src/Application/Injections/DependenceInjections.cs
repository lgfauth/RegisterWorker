using Application.Consumer.Connection;
using Application.Consumer;
using Application.Interfaces;
using Application.Services;
using Domain.Models;
using MicroservicesLogger;
using MicroservicesLogger.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Repository.Interfaces;
using Repository.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Application.Injections
{
    [ExcludeFromCodeCoverage]
    public class DependenceInjections
    {
        public static void Injections(IServiceCollection services, string mongoDbConnectionString)
        {

            services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoDbConnectionString));

            services.AddSingleton<IWorkerLog<WorkerLogModel>, WorkerLog<WorkerLogModel>>();

            services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
            services.AddSingleton<IUnsubscriptionRepository, UnsubscriptionRepository>();

            services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();

            services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();

            services.AddSingleton<ISubscriptionService, SubscriptionService>();
            services.AddSingleton<IUnsubscriptionService, UnsubscriptionService>();

            services.AddDataProtection();
            services.AddTransient<IEmailSender, EmailSender>();
        }
    }
}
