using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace Crypto.Providers
{
    public class RsaKeyPairProvider : IKeyProvider
    {
        private readonly IConfiguration configuration;
        private readonly RsaGenerator rsaGenerator;
        private readonly SecureRandomGenerator secureRandom;

        public RsaKeyPairProvider(IConfiguration configuration, RsaGenerator rsaGenerator, SecureRandomGenerator secureRandom)
        {
            this.configuration = configuration;
            this.rsaGenerator = rsaGenerator;
            this.secureRandom = secureRandom;
        }

        public IAsymmetricKey CreateAsymmetricKeyPair(string password, int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaGenerator.GenerateKeyPair(keySize);

            var salt = new byte[keySize];
            secureRandom.NextBytes(salt);

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(rsaKeyPair.Private);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            EncryptedPrivateKeyInfo encryptedPrivateKeyInfo = EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo("PBEwithSHA-256and256bitAES-CBC-BC", password.ToCharArray(), salt, iterationCount, privateKeyInfo);
            byte[] privateKey = encryptedPrivateKeyInfo.GetDerEncoded();

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKey = publicKeyInfo.GetDerEncoded();

            int keyLength = ((RsaKeyParameters) rsaKeyPair.Private).Modulus.BitLength;
            return new RsaKeyPair(privateKey, publicKey, keyLength);
        }
    }
}