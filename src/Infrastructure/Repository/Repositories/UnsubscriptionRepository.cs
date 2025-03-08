using Domain.Entities;
using Domain.Models.Envelope;
using Domain.Settings;
using MongoDB.Driver;
using Repository.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Repository.Repositories
{
    [ExcludeFromCodeCoverage]
    public class UnsubscriptionRepository : RepositoryBase, IUnsubscriptionRepository
    {
        public UnsubscriptionRepository(EnvirolmentVariables envirolmentVariables) : base(envirolmentVariables)
        {
        }

        public async Task<IResponse<bool>> DeleteUserAsync(User user)
        {
            var filtro = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Username, user.Username),
                Builders<User>.Filter.Eq(u => u.PasswordHash, user.PasswordHash),
                Builders<User>.Filter.Eq(u => u.Email, user.Email)
            );

            var resultado = await _users.DeleteOneAsync(filtro);

            return new ResponseOk<bool>(resultado.DeletedCount <= 0);
        }
    }
}