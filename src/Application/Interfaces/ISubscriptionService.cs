using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Interfaces
{
    public interface ISubscriptionService
    {
        public Task<IResponse<bool>> ProcessSubscription(UserQueueRegister userQueueRegister, string logId);
    }
}