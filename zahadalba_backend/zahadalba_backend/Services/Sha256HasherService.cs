using System.Security.Cryptography;
using System.Text;

namespace zahadalba_backend.Services
{
    public class Sha256HasherService : ISha256HasherService
    {
        public string GetHash(string rawData)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes);
        }
    }
}
