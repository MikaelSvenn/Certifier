using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Crypto.Providers
{
    public class RsaKeyPairProvider : IKeyProvider
    {
        private readonly IConfiguration configuration;
        private readonly RsaKeyPairGenerator rsaKeyPairGenerator;
        private readonly SecureRandomGenerator secureRandom;

        public RsaKeyPairProvider(IConfiguration configuration, RsaKeyPairGenerator rsaKeyPairGenerator, SecureRandomGenerator secureRandom)
        {
            this.configuration = configuration;
            this.rsaKeyPairGenerator = rsaKeyPairGenerator;
            this.secureRandom = secureRandom;
        }

        public IAsymmetricKey CreateAsymmetricKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaKeyPairGenerator.GenerateKeyPair(keySize);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKey = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(rsaKeyPair.Private);
            byte[] privateKey = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            int keyLength = ((RsaKeyParameters) rsaKeyPair.Private).Modulus.BitLength;
            return new RsaKeyPair(privateKey, publicKey, keyLength, AsymmetricKeyType.Rsa);
        }

        public IAsymmetricKey CreateAsymmetricPkcs12KeyPair(string password, int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaKeyPairGenerator.GenerateKeyPair(keySize);

            var salt = new byte[keySize];
            secureRandom.NextBytes(salt);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            byte[] privateKey = PrivateKeyFactory.EncryptKey("PBEwithSHA-1and3-keyDESEDE-CBC",
                password.ToCharArray(), salt, iterationCount, rsaKeyPair.Private);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKey = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            int keyLength = ((RsaKeyParameters) rsaKeyPair.Private).Modulus.BitLength;
            return new RsaKeyPair(privateKey, publicKey, keyLength, AsymmetricKeyType.RsaPkcs12);
        }
    }
}