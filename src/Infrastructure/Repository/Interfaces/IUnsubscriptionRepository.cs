using Domain.Entities;

namespace Repository.Interfaces
{
    public interface IUnsubscriptionRepository
    {
        Task<User?> DeleteUserAsync(User user, string logId);
    }
}