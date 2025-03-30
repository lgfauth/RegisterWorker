using Microsoft.AspNetCore.DataProtection;
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
        /// <param name="protector">IDataProtector object</param>
        /// <returns>Encripted token string</returns>
        public static string TokenGenAndEncprtor(string email, string? name, IDataProtector protector)
        {
            var tokenData = $"{email}|{name}|{DateTime.UtcNow:o}";

            return protector.Protect(tokenData);
        }

        /// <summary>
        /// Decriptor for token data, used to confirm subscription
        /// </summary>
        /// <param name="encryptedToken">string token</param>
        /// <param name="protector">IDataProtector object</param>
        /// <returns></returns>
        public static string DecryptToken(string encryptedToken, IDataProtector protector)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedToken);
            byte[] decryptedBytes = protector.Unprotect(encryptedBytes);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}