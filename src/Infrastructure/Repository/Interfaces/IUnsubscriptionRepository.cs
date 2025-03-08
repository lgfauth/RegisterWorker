using Domain.Entities;
using Domain.Models.Envelope;

namespace Repository.Interfaces
{
    public interface IUnsubscriptionRepository
    {
        Task<IResponse<bool>> DeleteUserAsync(User user);
    }
}