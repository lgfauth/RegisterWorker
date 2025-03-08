using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using MicroservicesLogger.Interfaces;
using MicroservicesLogger.Models;
using Repository.Interfaces;

namespace Application.Services
{
    public class UnsubscriptionService : IUnsubscriptionService
    {
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly IUnsubscriptionRepository _unsubscriptionRepository;

        public UnsubscriptionService(IWorkerLog<WorkerLogModel> logger, IUnsubscriptionRepository unsubscriptionRepository)
        {
            _logger = logger;
            _unsubscriptionRepository = unsubscriptionRepository;
        }

        public async Task<IResponse<bool>> ProcessUnsubscription(UserQueueRegister userQueueRegister, string logId)
        {
            var baselog = await _logger.GetBaseLogAsync(logId);
            var sublog = new SubLog();

            try
            {
                sublog.StartCronometer();

                var user = new User(userQueueRegister);

                var response = await _unsubscriptionRepository.DeleteUserAsync(user);

                sublog.StopCronometer();

                return response;
            }
            catch (Exception ex)
            {
                sublog.StopCronometer();
                sublog.Exception = ex;

                return new ResponseError<bool>(new ResponseModel { Code = "RB502", Message = ex.Message });
            }
            finally
            {
                await baselog.AddStepAsync("DATABASE_DELETE_USER", sublog);
            }
        }
    }
}
