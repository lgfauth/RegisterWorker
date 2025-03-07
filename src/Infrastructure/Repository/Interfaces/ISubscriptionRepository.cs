using Domain.Entities;

namespace Repository.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<User?> InsertNewUserAsync(User user, string logId);
    }
}