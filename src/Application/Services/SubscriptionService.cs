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
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(IWorkerLog<WorkerLogModel> logger, ISubscriptionRepository subscriptionRepository)
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<IResponse<bool>> ProcessSubscription(UserQueueRegister userQueueRegister, string logId)
        {
            var baselog = await _logger.GetBaseLogAsync(logId);
            var sublog = new SubLog();

            try
            {
                sublog.StartCronometer();

                var user = new User(userQueueRegister);

                var response = await _subscriptionRepository.InsertNewUserAsync(user);

                sublog.StopCronometer();

                return response;
            }
            catch (Exception ex)
            {
                sublog.StopCronometer();
                sublog.Exception = ex;

                return new ResponseError<bool>(new ResponseModel { Code = "RB501", Message = ex.Message });
            }
            finally
            {
                await baselog.AddStepAsync("DATABASE_INSERT_NEW_USER", sublog);
            }
        }
    }
}
