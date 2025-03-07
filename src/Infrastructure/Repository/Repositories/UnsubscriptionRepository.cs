using Domain.Entities;
using Domain.Settings;
using Repository.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Repository.Repositories
{
    [ExcludeFromCodeCoverage]
    public class UnsubscriptionRepository : RepositoryBase, IUnsubscriptionRepository
    {
        public UnsubscriptionRepository(EnvirolmentVariables envirolmentVariables) : base(envirolmentVariables) { }

        public Task<User?> DeleteUserAsync(User user, string logId)
        {
            throw new NotImplementedException();
        }
    }
}