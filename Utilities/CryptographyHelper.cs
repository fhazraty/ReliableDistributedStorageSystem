namespace Utilities
{
    using MessagePack; //Install-Package MessagePack -Version 2.3.85
    using System.Security.Cryptography;
    using System.Text;
    public class CryptographyHelper
    {
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
            var start = DateTime.Now;

            var param = new ECParameters
            {
                D = privateKey,
                Curve = Curve,
            };

            using (var dsa = ECDsa.Create(param))
            {
                var signedHash = dsa.SignHash(hash);
                
                var miliseconds = DateTime.Now.Subtract(start).TotalMilliseconds;

                return signedHash;
            }
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