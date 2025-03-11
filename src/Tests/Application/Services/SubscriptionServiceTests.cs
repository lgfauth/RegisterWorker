﻿using Application.Services;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using MicroservicesLogger.Interfaces;
using Moq;
using Repository.Interfaces;
using Xunit;

namespace RegisterWorkerTests.Application.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IWorkerLog<WorkerLogModel>> _loggerMock;
        private readonly Mock<ISubscriptionRepository> _repositoryMock;
        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _loggerMock = new Mock<IWorkerLog<WorkerLogModel>>();
            _repositoryMock = new Mock<ISubscriptionRepository>();
            _service = new SubscriptionService(_loggerMock.Object, _repositoryMock.Object);
        }

        [Fact(DisplayName = "Deve inserir usuário com sucesso")]
        public async Task ProcessSubscription_ShouldInsertUserSuccessfully()
        {
            // Arrange
            var userQueueRegister = new UserQueueRegister { };
            var user = new User(userQueueRegister);
            var logId = Guid.NewGuid().ToString();

            _loggerMock.Setup(l => l.GetBaseLogAsync(logId)).ReturnsAsync(new WorkerLogModel());

            _repositoryMock.Setup(r => r.InsertNewUserAsync(It.IsAny<User>()))
                           .ReturnsAsync(new ResponseOk<bool>(true));

            // Act
            var result = await _service.ProcessSubscription(userQueueRegister, logId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);

            _repositoryMock.Verify(r => r.InsertNewUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact(DisplayName = "Deve retornar erro quando ocorrer exceção")]
        public async Task ProcessSubscription_ShouldReturnErrorWhenExceptionOccurs()
        {
            // Arrange
            var userQueueRegister = new UserQueueRegister { };
            var logId = Guid.NewGuid().ToString();
            var exceptionMessage = "Erro ao inserir no banco.";

            _loggerMock.Setup(l => l.GetBaseLogAsync(logId)).ReturnsAsync(new WorkerLogModel());

            _repositoryMock.Setup(r => r.InsertNewUserAsync(It.IsAny<User>()))
                           .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.ProcessSubscription(userQueueRegister, logId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(exceptionMessage, result.Error.Message);
            Assert.Equal("RB501", result.Error.Code);

            _repositoryMock.Verify(r => r.InsertNewUserAsync(It.IsAny<User>()), Times.Once);
        }
    }
}
