using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Interfaces
{
    public interface IUnsubscriptionService
    {
        public Task<IResponse<bool>> ProcessUnsubscription(UserQueueRegister userQueueRegister, string logId);
    }
}