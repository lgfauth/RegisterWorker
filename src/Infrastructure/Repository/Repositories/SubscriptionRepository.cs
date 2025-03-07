using Domain.Entities;
using Domain.Settings;
using Repository.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Repository.Repositories
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
    {
        public SubscriptionRepository(EnvirolmentVariables envirolmentVariables) : base(envirolmentVariables) { }

        public Task<User?> InsertNewUserAsync(User user, string logId)
        {
            throw new NotImplementedException();
        }
    }
}