using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using Domain.Settings;
using MicroservicesLogger.Enums;
using MicroservicesLogger.Interfaces;
using MicroservicesLogger.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RegisterWorker
{
    public class Worker : BackgroundService
    {
        private const string _typeRegister = "REGISTER";
        private const int _retryDelayMs = 15000;
        private const int _retryMaxCount = 20;

        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private readonly EnvirolmentVariables _variables;
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUnsubscriptionService _unsubscriptionService;

        public Worker(IWorkerLog<WorkerLogModel> logger, ISubscriptionService subscriptionService, IUnsubscriptionService unsubscriptionService, EnvirolmentVariables variables)
        {
            _logger = logger;
            _variables = variables;
            _subscriptionService = subscriptionService;
            _unsubscriptionService = unsubscriptionService;

            var factory = new ConnectionFactory()
            {
                HostName = _variables.RABBITMQCONFIGURATION_HOSTNAME,
                UserName = _variables.RABBITMQCONFIGURATION_USERNAME,
                Password = _variables.RABBITMQCONFIGURATION_PASSWORD,
                VirtualHost = _variables.RABBITMQCONFIGURATION_VIRTUALHOST
            };

            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.QueueDeclareAsync(
                arguments: null,
                durable: true,
                exclusive: false,
                autoDelete: false,
                queue: _variables.RABBITMQCONFIGURATION_QUEUENAME
            );

            _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 3, global: false);

            //// Fila de retry com TTL e redirecionamento
            //var retryQueueArgs = new Dictionary<string, object>
            //{
            //    { "x-message-ttl", _retryDelayMs }, // Tempo que a mensagem fica na fila antes de ser reencaminhada
            //    { "x-dead-letter-exchange", "" },   // Enviar de volta para o exchange padrão
            //    { "x-dead-letter-routing-key", _variables.RABBITMQCONFIGURATION_QUEUENAME } // Redirecionar para a fila principal após o delay
            //};

            //_channel.QueueDeclareAsync(queue: _variables.RABBITMQCONFIGURATION_QUEUENAME, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs!);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var baselog = await _logger.CreateBaseLogAsync();
                    var sublog = new SubLog();
                    var logType = LogTypes.INFO;

                    await baselog.AddStepAsync("CONSUME_REGISTER_MESSAGE", sublog);

                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        if (message is null)
                        {
                            baselog.Response = "Received message is null.";
                            logType = LogTypes.WARN;

                            await _channel.AbortAsync();
                        }

                        baselog.Request = message;

                        UserQueueRegister userQueueRegister = JsonConvert.DeserializeObject<UserQueueRegister>(message!)!;
                        IResponse<bool> response = null!;

                        if (userQueueRegister.Type!.Equals(_typeRegister))
                            response = await _subscriptionService.ProcessSubscription(userQueueRegister, baselog.Id);
                        else
                            response = await _unsubscriptionService.ProcessUnsubscription(userQueueRegister, baselog.Id);

                        if (!response.IsSuccess)
                        {
                            baselog.Response = response.Error;
                            logType = LogTypes.WARN;

                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);

                            await _channel.BasicPublishAsync(
                                exchange: "",
                                routingKey: _variables.RABBITMQCONFIGURATION_QUEUENAME,
                                body: ea.Body.ToArray(),
                                stoppingToken
                            );
                        }

                        baselog.Response = "Success";

                        // Confirm the process of received message
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        sublog.Exception = ex;
                        logType = LogTypes.ERROR;

                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);

                        await _channel.BasicPublishAsync(
                                exchange: "",
                                routingKey: _variables.RABBITMQCONFIGURATION_QUEUENAME,
                                body: ea.Body.ToArray(),
                                stoppingToken
                            );
                    }
                    finally
                    {
                        await _logger.WriteLogAsync(logType, baselog);
                    }
                };

                await _channel.BasicConsumeAsync(
                    consumer: consumer,
                    autoAck: false,
                    queue: _variables.RABBITMQCONFIGURATION_QUEUENAME
                );
            }
        }

        public override void Dispose()
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            base.Dispose();
        }
    }
}