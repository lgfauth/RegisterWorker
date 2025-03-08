using Domain.Entities;
using Domain.Models.Envelope;

namespace Repository.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<IResponse<bool>> InsertNewUserAsync(User user);
    }
}