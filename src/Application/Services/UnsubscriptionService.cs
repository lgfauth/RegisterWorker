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
        private readonly IEmailSender _emailSender;
        private readonly IWorkerLog<WorkerLogModel> _logger;
        private readonly IUnsubscriptionRepository _unsubscriptionRepository;

        public UnsubscriptionService(IWorkerLog<WorkerLogModel> logger, IUnsubscriptionRepository unsubscriptionRepository, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
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

                _ = await _unsubscriptionRepository.DeleteUserAsync(user);

                sublog.StopCronometer();

                var sublogEmail = new SubLog();
                await baselog.AddStepAsync("SEND_DELETE_CONFIRMATION_EMAIL", sublogEmail);

                sublogEmail.StartCronometer();

                var response = await _emailSender.SendDeletionConfirmationEmailAsync(userQueueRegister);

                sublogEmail.StopCronometer();

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
