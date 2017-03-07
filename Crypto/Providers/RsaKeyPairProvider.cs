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

        public IAsymmetricKeyPair CreateAsymmetricKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaKeyPairGenerator.GenerateKeyPair(keySize);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKeyContent = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(rsaKeyPair.Private);
            byte[] privateKeyContent = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            int privateKeyLength = ((RsaKeyParameters) rsaKeyPair.Private).Modulus.BitLength;
            int publicKeyLength = ((RsaKeyParameters) rsaKeyPair.Public).Modulus.BitLength;

            var publicKey = new RsaKey(publicKeyContent, AsymmetricKeyType.Rsa, privateKeyLength);
            var privateKey = new RsaKey(privateKeyContent, AsymmetricKeyType.Rsa, publicKeyLength);

            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        public IAsymmetricKeyPair CreateAsymmetricPkcs12KeyPair(string password, int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaKeyPairGenerator.GenerateKeyPair(keySize);

            var saltLength = configuration.Get<int>("SaltLengthInBytes");
            var salt = new byte[saltLength];
            secureRandom.NextBytes(salt);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            byte[] privateKeyContent = PrivateKeyFactory.EncryptKey("PBEwithSHA-1and3-keyDESEDE-CBC",
                password.ToCharArray(), salt, iterationCount, rsaKeyPair.Private);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKeyContent = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            int privateKeyLength = ((RsaKeyParameters) rsaKeyPair.Private).Modulus.BitLength;
            int publicKeyLength = ((RsaKeyParameters) rsaKeyPair.Public).Modulus.BitLength;

            var publicKey = new RsaKey(publicKeyContent, AsymmetricKeyType.Rsa, privateKeyLength);
            var privateKey = new RsaKey(privateKeyContent, AsymmetricKeyType.RsaPkcs12, publicKeyLength);

            return new AsymmetricKeyPair(privateKey, publicKey);
        }
    }
}