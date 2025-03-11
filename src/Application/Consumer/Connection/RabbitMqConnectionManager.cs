using Application.Interfaces;
using Domain.Settings;
using RabbitMQ.Client;

namespace Application.Consumer.Connection
{
    public class RabbitMqConnectionManager : IRabbitMqConnectionManager
    {
        private readonly EnvirolmentVariables _variables;
        private readonly ushort _prefetchCount;
        private IConnection _connection;
        private IChannel _channel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variables"></param>
        public RabbitMqConnectionManager(EnvirolmentVariables variables)
        {
            _variables = variables;
            _prefetchCount = 3;
        }

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _variables.RABBITMQCONFIGURATION_HOSTNAME,
                UserName = _variables.RABBITMQCONFIGURATION_USERNAME,
                Password = _variables.RABBITMQCONFIGURATION_PASSWORD,
                VirtualHost = _variables.RABBITMQCONFIGURATION_VIRTUALHOST
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            try
            {
                await _channel.QueueDeclarePassiveAsync(_variables.RABBITMQCONFIGURATION_QUEUENAME);
            }
            catch
            {
                await _channel.QueueDeclareAsync(
                    queue: _variables.RABBITMQCONFIGURATION_QUEUENAME,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "" },
                        { "x-dead-letter-routing-key", _variables.RABBITMQCONFIGURATION_RETRY_QUEUENAME }
                    }!
                );
            }

            try
            {
                await _channel.QueueDeclarePassiveAsync(_variables.RABBITMQCONFIGURATION_RETRY_QUEUENAME);
            }
            catch
            {
                await _channel.QueueDeclareAsync(
                    queue: _variables.RABBITMQCONFIGURATION_RETRY_QUEUENAME,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "" },
                        { "x-message-ttl", (long)TimeSpan.FromMinutes(15).TotalMilliseconds },
                        { "x-dead-letter-routing-key", _variables.RABBITMQCONFIGURATION_DLQ_QUEUENAME }
                    }!
                );
            }

            try
            {
                await _channel.QueueDeclarePassiveAsync(_variables.RABBITMQCONFIGURATION_DLQ_QUEUENAME);
            }
            catch
            {
                await _channel.QueueDeclareAsync(
                queue: _variables.RABBITMQCONFIGURATION_DLQ_QUEUENAME,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            }

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _prefetchCount, global: false);
        }

        public IConnection GetConnection() => _connection;

        public IChannel GetChannel() => _channel;

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }
}
