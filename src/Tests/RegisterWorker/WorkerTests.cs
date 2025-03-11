using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using Domain.Settings;
using MicroservicesLogger.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Moq;
using System.Text;
using Xunit;
using RegisterWorker;

namespace RegisterWorkerTests.RegisterWorker
{
    public class WorkerTests
    {
        private readonly Mock<IWorkerLog<WorkerLogModel>> _loggerMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IUnsubscriptionService> _unsubscriptionServiceMock;
        private readonly Mock<IChannel> _channelMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly EnvirolmentVariables _variables;
        private readonly Worker _worker;

        public WorkerTests()
        {
            _loggerMock = new Mock<IWorkerLog<WorkerLogModel>>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _unsubscriptionServiceMock = new Mock<IUnsubscriptionService>();
            _channelMock = new Mock<IChannel>();
            _connectionMock = new Mock<IConnection>();

            _variables = new EnvirolmentVariables
            {
                RABBITMQCONFIGURATION_QUEUENAME = "test-queue"
            };

            _worker = new Worker(
                _loggerMock.Object,
                _subscriptionServiceMock.Object,
                _unsubscriptionServiceMock.Object,
                _variables
            );
        }

        [Fact(DisplayName = "Deve processar mensagem de subscription com sucesso")]
        public async Task ProcessMessage_ShouldAck_OnSuccess()
        {
            // Arrange
            var message = new UserQueueRegister { Type = "REGISTER" };
            var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var ea = new BasicDeliverEventArgs { Body = new ReadOnlyMemory<byte>(body) };

            _subscriptionServiceMock.Setup(x => x.ProcessSubscription(It.IsAny<UserQueueRegister>(), It.IsAny<string>()))
                .ReturnsAsync(new ResponseOk<bool>(true));

            // Act
            await _worker.ExecuteTask(ea, CancellationToken.None);

            // Assert
            _channelMock.Verify(x => x.BasicAckAsync(It.IsAny<ulong>(), false), Times.Once);
            _channelMock.Verify(x => x.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Deve reprocessar mensagem se o processamento falhar")]
        public async Task ProcessMessage_ShouldRetry_OnFailure()
        {
            // Arrange
            var message = new UserQueueRegister { Type = "REGISTER" };
            var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var ea = new BasicDeliverEventArgs { Body = new ReadOnlyMemory<byte>(body) };

            _subscriptionServiceMock.Setup(x => x.ProcessSubscription(It.IsAny<UserQueueRegister>(), It.IsAny<string>()))
                .ReturnsAsync(new ResponseError<bool>(new ResponseModel { Code = "ERROR", Message = "Erro" }));

            // Act
            await _worker.ProcessMessageAsync(ea, CancellationToken.None);

            // Assert
            _channelMock.Verify(x => x.BasicNackAsync(It.IsAny<ulong>(), false, false), Times.Once);
            _channelMock.Verify(x => x.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Deve ignorar mensagens nulas ou vazias")]
        public async Task ProcessMessage_ShouldIgnore_NullMessage()
        {
            // Arrange
            var body = Encoding.UTF8.GetBytes("");
            var ea = new BasicDeliverEventArgs { Body = new ReadOnlyMemory<byte>(body) };

            // Act
            await _worker.ProcessMessageAsync(ea, CancellationToken.None);

            // Assert
            _channelMock.Verify(x => x.BasicAckAsync(It.IsAny<ulong>(), false), Times.Never);
            _channelMock.Verify(x => x.BasicNackAsync(It.IsAny<ulong>(), false, false), Times.Never);
        }

        [Fact(DisplayName = "Deve reprocessar mensagem em caso de exceção")]
        public async Task ProcessMessage_ShouldRetry_OnException()
        {
            // Arrange
            var message = new UserQueueRegister { Type = "REGISTER" };
            var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var ea = new BasicDeliverEventArgs { Body = new ReadOnlyMemory<byte>(body) };

            _subscriptionServiceMock.Setup(x => x.ProcessSubscription(It.IsAny<UserQueueRegister>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Erro inesperado"));

            // Act
            await _worker.ProcessMessageAsync(ea, CancellationToken.None);

            // Assert
            _channelMock.Verify(x => x.BasicNackAsync(It.IsAny<ulong>(), false, false), Times.Once);
            _channelMock.Verify(x => x.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
