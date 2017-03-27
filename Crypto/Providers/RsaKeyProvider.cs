using System;
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
    public class RsaKeyProvider : IAsymmetricKeyProvider<RsaKey>
    {
        private readonly IConfiguration configuration;
        private readonly RsaKeyPairGenerator rsaKeyPairGenerator;
        private readonly SecureRandomGenerator secureRandom;

        public RsaKeyProvider(IConfiguration configuration, RsaKeyPairGenerator rsaKeyPairGenerator, SecureRandomGenerator secureRandom)
        {
            this.configuration = configuration;
            this.rsaKeyPairGenerator = rsaKeyPairGenerator;
            this.secureRandom = secureRandom;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
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

            int privateKeyLength = GetKeyLength(rsaKeyPair.Private);
            int publicKeyLength = GetKeyLength(rsaKeyPair.Public);

            var publicKey = new RsaKey(publicKeyContent, AsymmetricKeyType.Public, publicKeyLength);
            var privateKey = new RsaKey(privateKeyContent, AsymmetricKeyType.Private, privateKeyLength);

            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key)
        {
            return ((RsaKeyParameters) key).Modulus.BitLength;
        }

        public IAsymmetricKeyPair CreatePkcs12KeyPair(string password, int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = rsaKeyPairGenerator.GenerateKeyPair(keySize);

            var saltLength = configuration.Get<int>("SaltLengthInBytes");
            byte[] salt = secureRandom.NextBytes(saltLength);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            byte[] privateKeyContent = PrivateKeyFactory.EncryptKey(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc,
                password.ToCharArray(), salt, iterationCount, rsaKeyPair.Private);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
            byte[] publicKeyContent = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            int privateKeyLength = GetKeyLength(rsaKeyPair.Private);
            int publicKeyLength = GetKeyLength(rsaKeyPair.Public);

            var publicKey = new RsaKey(publicKeyContent, AsymmetricKeyType.Public, publicKeyLength);
            var privateKey = new RsaKey(privateKeyContent, AsymmetricKeyType.Encrypted, privateKeyLength);

            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        public RsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            if (keyType == AsymmetricKeyType.Encrypted)
            {
                throw new InvalidOperationException("Unable to access encrypted key content");
            }

            AsymmetricKeyParameter key;
            try
            {
                key = keyType == AsymmetricKeyType.Public
                    ? PublicKeyFactory.CreateKey(content)
                    : PrivateKeyFactory.CreateKey(content);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Key type mismatch");
            }

            var keyLength = GetKeyLength(key);
            return new RsaKey(content, keyType, keyLength);
        }
    }
}