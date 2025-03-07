using Application.Interfaces;
using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Services
{
    public class UnsubscriptionService : IUnsubscriptionService
    {
        public Task<IResponse<bool>> ProcessUnsubscription(UserQueueRegister userQueueRegister, string logId)
        {
            throw new NotImplementedException();
        }
    }
}
