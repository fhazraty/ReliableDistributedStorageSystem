using System.Security.Cryptography;
using System.Text;

namespace SimpleTcpIpSendFile
{
    public class CryptographyHelper
    {
        public string GetHashSha256(byte[] data, int offset, int count)
        {
            SHA256 Sha256 = SHA256.Create();
            return UTF8Encoding.UTF8.GetString(Sha256.ComputeHash(data,offset,count));
        }
    }
}
