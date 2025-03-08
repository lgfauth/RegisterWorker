using Domain.Entities;
using Domain.Models.Envelope;
using Domain.Settings;
using Repository.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Repository.Repositories
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
    {
        public SubscriptionRepository(EnvirolmentVariables envirolmentVariables) : base(envirolmentVariables)
        {
        }

        public async Task<IResponse<bool>> InsertNewUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return new ResponseOk<bool>(true);
        }
    }
}