namespace Utilities
{
    using MessagePack; //Install-Package MessagePack -Version 2.3.85
    using System.Security.Cryptography;
    using System.Text;
    public class CryptographyHelper
    {
        /// <summary>
        /// ECC
        /// </summary>
        public readonly ECCurve Curve = ECCurve.NamedCurves.nistP256;
        public string GetHashSha256(byte[] data, int offset, int count)
        {
            SHA256 Sha256 = SHA256.Create();
            return UTF8Encoding.UTF8.GetString(Sha256.ComputeHash(data, offset, count));
        }
        public byte[] GetHashSha256ToByte(byte[] data, int offset, int count)
        {
            SHA256 Sha256 = SHA256.Create();
            return Sha256.ComputeHash(data, offset, count);
        }
        public void GenerateKey(out byte[] privateKey, out byte[] publicKey)
        {
            ECParameters param;
            using (var dsa = ECDsa.Create(Curve))
                param = dsa.ExportParameters(true);

            privateKey = param.D;
            publicKey = ToBytes(param.Q);
        }
        public byte[] Sign(byte[] hash, byte[] privateKey, byte[] publicKey)
        {
            var param = new ECParameters
            {
                D = privateKey,
                Q = ToEcPoint(publicKey),
                Curve = Curve,
            };

            using (var dsa = ECDsa.Create(param))
                return dsa.SignHash(hash);
        }
        public byte[] Sign(byte[] hash, byte[] privateKey)
        {
            var param = new ECParameters
            {
                D = privateKey,
                Curve = Curve,
            };

            using (var dsa = ECDsa.Create(param))
                return dsa.SignHash(hash);
        }
        public bool Verify(byte[] hash, byte[] signature, byte[] publicKey)
        {
            var param = new ECParameters
            {
                Q = ToEcPoint(publicKey),
                Curve = Curve,
            };

            using (var dsa = ECDsa.Create(param))
                return dsa.VerifyHash(hash, signature);
        }
        public bool TestKey(byte[] privateKey, byte[] publicKey)
        {
            byte[] testHash;
            using (var sha = SHA256.Create())
                testHash = sha.ComputeHash(new byte[0]);

            try
            {
                var signature = Sign(testHash, privateKey, publicKey);
                return Verify(testHash, signature, publicKey);
            }
            catch { return false; }
        }
        public byte[] ToBytes(ECPoint point)
        {
            return MessagePackSerializer.Serialize(new FormattableEcPoint { X = point.X, Y = point.Y });
        }
        public ECPoint ToEcPoint(byte[] bytes)
        {
            var pt = MessagePackSerializer.Deserialize<FormattableEcPoint>(bytes);

            return new ECPoint { X = pt.X, Y = pt.Y };
        }
        /// <summary>
        /// RSA
        /// </summary>

        public void GenerateKeyRSA(out byte[] privateKey, out byte[] publicKey)
        {
            RSA rsa = new RSACryptoServiceProvider(2048);// Generate a new 2048 bit RSA key

            string publicPrivateKeyXML = rsa.ToXmlString(true);
            privateKey = Encoding.UTF8.GetBytes(publicPrivateKeyXML);


            string publicOnlyKeyXML = rsa.ToXmlString(false);
            publicKey = Encoding.UTF8.GetBytes(publicPrivateKeyXML);
        }
        public byte[] SignRSA(byte[] data, byte[] privateKey)
        {
            //// The array to store the signed message in bytes
            byte[] signedBytes;
            using (var rsa = new RSACryptoServiceProvider())
            {
                //// Write the message to a byte array using UTF8 as the encoding.
                var encoder = new UTF8Encoding();
                byte[] originalData = data;

                try
                {
                    //// Import the private key used for signing the message
                    rsa.FromXmlString(Encoding.UTF8.GetString(privateKey));

                    //// Sign the data, using SHA512 as the hashing algorithm 
                    signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA512"));
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    //// Set the keycontainer to be cleared when rsa is garbage collected.
                    rsa.PersistKeyInCsp = false;
                }
            }
            //// Convert the a base64 string before returning
            return signedBytes;
        }
        public bool VerifyRSA(byte[] originalMessage, byte[] signedMessage, byte[] publicKey)
        {
            bool success = false;
            using (var rsa = new RSACryptoServiceProvider())
            {
                byte[] bytesToVerify = originalMessage;
                byte[] signedBytes = signedMessage;
                try
                {
                    rsa.FromXmlString(Encoding.UTF8.GetString(publicKey));

                    SHA512Managed Hash = new SHA512Managed();

                    byte[] hashedData = Hash.ComputeHash(signedBytes);

                    success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA512"), signedBytes);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
            return success;
        }
    }
    [MessagePackObject]
    public class FormattableEcPoint
    {
        [Key(0)]
        public virtual byte[] X { get; set; }

        [Key(1)]
        public virtual byte[] Y { get; set; }
    }
}