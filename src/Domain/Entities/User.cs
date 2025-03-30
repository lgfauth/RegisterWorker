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
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; } = "Pending confirmation";
    }
}