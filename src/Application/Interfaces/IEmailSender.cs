using Domain.Entities;
using Domain.Models.Envelope;

namespace Application.Interfaces
{
    public interface IEmailSender
    {
        public Task<IResponse<bool>> SendConfirmationEmailAsync(UserQueueRegister userQueueRegister, string type);
    }
}
