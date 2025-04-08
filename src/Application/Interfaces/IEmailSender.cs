using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Interfaces
{
    public interface IEmailSender
    {
        public Task<IResponse<bool>> SendSubscriptionConfirmationEmailAsync(UserQueueRegister userQueueRegister);
        public Task<IResponse<bool>> SendDeletionConfirmationEmailAsync(UserQueueRegister userQueueRegister);
    }
}
