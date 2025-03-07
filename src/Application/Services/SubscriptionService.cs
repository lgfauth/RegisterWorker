using Application.Interfaces;
using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        public Task<IResponse<bool>> ProcessSubscription(UserQueueRegister userQueueRegister, string logId)
        {
            throw new NotImplementedException();
        }
    }
}
