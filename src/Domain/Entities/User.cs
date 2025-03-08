using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    /// <summary>
    /// MongoDB User entity
    /// </summary>
    public class User
    {
        public User(UserQueueRegister userQueueRegister)
        {
            Name = userQueueRegister.Name!;
            Email = userQueueRegister.Email;
            Username = userQueueRegister.Username;
            LastName = userQueueRegister.LastName!;
            PasswordHash = userQueueRegister.Password;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}