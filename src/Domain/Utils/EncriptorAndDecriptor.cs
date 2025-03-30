using System.Security.Cryptography;
using System.Text;

namespace Domain.Utils
{
    public class EncriptorAndDecriptor
    {
        /// <summary>
        /// Encript data and generate token for user confirm subscription
        /// </summary>
        /// <param name="email">Email used on registration</param>
        /// <param name="name">Name used on registration</param>
        /// <returns>Encripted token string</returns>
        public static string TokenGenAndEncprtor(string email, string? name)
        {
            var tokenData = $"{email}|{name}|{DateTime.UtcNow:o}";

            byte[] plainBytes = Encoding.UTF8.GetBytes(tokenData);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.LocalMachine);

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Decriptor for token data, used to confirm subscription
        /// </summary>
        /// <param name="encryptedToken">string token</param>
        /// <returns></returns>
        public static string DecryptToken(string encryptedToken)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedToken);
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}