using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using MicroservicesLogger.Interfaces;
using MicroservicesLogger.Models;
using Repository.Interfaces;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IEmailSender _emailSender;
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(IWorkerLog<WorkerLogModel> logger, ISubscriptionRepository subscriptionRepository, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<IResponse<bool>> ProcessSubscription(UserQueueRegister userQueueRegister, string logId)
        {
            var baselog = await _logger.GetBaseLogAsync(logId);
            var sublog = new SubLog();
            await baselog.AddStepAsync("DATABASE_INSERT_NEW_USER", sublog);

            try
            {
                sublog.StartCronometer();

                var user = new User(userQueueRegister);

                _ = await _subscriptionRepository.InsertNewUserAsync(user);

                sublog.StopCronometer();

                var sublogEmail = new SubLog();
                await baselog.AddStepAsync("SEND_CONFIRMATION_EMAIL", sublogEmail);

                sublogEmail.StartCronometer();

                var response = await _emailSender.SendConfirmationEmailAsync(userQueueRegister, "Register");
                
                sublogEmail.StopCronometer();

                return response;
            }
            catch (Exception ex)
            {
                sublog.StopCronometer();
                sublog.Exception = ex;

                return new ResponseError<bool>(new ResponseModel { Code = "RB501", Message = ex.Message });
            }
        }
    }
}