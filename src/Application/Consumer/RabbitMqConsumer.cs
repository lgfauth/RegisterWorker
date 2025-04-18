﻿using Application.Interfaces;
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

namespace Application.Consumer
{
    public class RabbitMqConsumer : IRabbitMqConsumer
    {
        private const string _typeRegister = "REGISTER";
        private const int _retryCounter = 10;

        private readonly EnvirolmentVariables _variables;
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUnsubscriptionService _unsubscriptionService;
        private readonly IRabbitMqConnectionManager _connectionManager;

        public RabbitMqConsumer(
            ISubscriptionService subscriptionService,
            IUnsubscriptionService unsubscriptionService,
            IWorkerLog<WorkerLogModel> logger,
            EnvirolmentVariables variables,
            IRabbitMqConnectionManager connectionManager)
        {
            _logger = logger;
            _variables = variables;
            _connectionManager = connectionManager;
            _subscriptionService = subscriptionService;
            _unsubscriptionService = unsubscriptionService;
        }

        public async Task StartConsumingAsync(CancellationToken stoppingToken)
        {
            var channel = _connectionManager.GetChannel();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var baselog = await _logger.CreateBaseLogAsync();
                var sublog = new SubLog();
                var logType = LogTypes.INFO;

                await baselog.AddStepAsync("CONSUME_REGISTER_MESSAGE", sublog);

                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    if (string.IsNullOrEmpty(message))
                    {
                        baselog.Response = "Received message is null.";
                        logType = LogTypes.WARN;

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                        return;
                    }

                    baselog.Request = message;
                    UserQueueRegister userQueueRegister = JsonConvert.DeserializeObject<UserQueueRegister>(message)!;
                    
                    IResponse<bool> response;

                    if (userQueueRegister.Type!.Equals(_typeRegister))
                        response = await _subscriptionService.ProcessSubscription(userQueueRegister, baselog.Id);
                    else
                        response = await _unsubscriptionService.ProcessUnsubscription(userQueueRegister, baselog.Id);

                    if (!response.IsSuccess)
                    {
                        baselog.Response = response.Error;
                        logType = LogTypes.WARN;
                    
                        await RetryMessageAsync(ea, channel, baselog.Id, stoppingToken);
                        
                        return;
                    }

                    baselog.Response = "Success";

                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    sublog.Exception = ex;
                    logType = LogTypes.ERROR;

                    await RetryMessageAsync(ea, channel, baselog.Id, stoppingToken);
                }
                finally
                {
                    await _logger.WriteLogAsync(logType, baselog);
                }
            };

            await channel.BasicConsumeAsync(
                queue: _variables.RABBITMQCONFIGURATION_QUEUENAME!,
                autoAck: false,
                consumer: consumer);

            await Task.CompletedTask;
        }

        private async Task RetryMessageAsync(BasicDeliverEventArgs ea, IChannel channel, string logId, CancellationToken stoppingToken)
        {
            var baselog = await _logger.GetBaseLogAsync(logId);
            var sublog = new SubLog();
            await baselog.AddStepAsync("SEND_MESSAGE_TO_RETRY_OR_DLQ", sublog);

            sublog.StartCronometer();

            var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>()!;
            var retryCount = headers.ContainsKey("x-retry-count") ? Convert.ToInt32(headers["x-retry-count"]) : 0;
            retryCount++;

            var properties = new BasicProperties
            {
                Headers = new Dictionary<string, object>
                {
                    { "x-retry-count", retryCount }
                }!,
                Persistent = true
            };

            var targetQueue = retryCount >= _retryCounter
                ? _variables.RABBITMQCONFIGURATION_DLQ_QUEUENAME
                : _variables.RABBITMQCONFIGURATION_RETRY_QUEUENAME;

            await channel.BasicPublishAsync(
                    exchange: "",
                    mandatory: false,
                    routingKey: targetQueue,
                    body: ea.Body.ToArray(),
                    basicProperties: properties,
                    cancellationToken: stoppingToken
                );

            await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);

            sublog.StopCronometer();
        }
    }
}