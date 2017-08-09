using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Crypto.Providers
{
    public class RsaKeyProvider : BCKeyProvider, IKeyProvider<RsaKey>
    {
        private readonly AsymmetricKeyPairGenerator asymmetricKeyPairGenerator;

        public RsaKeyProvider(AsymmetricKeyPairGenerator asymmetricKeyPairGenerator)
        {
            this.asymmetricKeyPairGenerator = asymmetricKeyPairGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = asymmetricKeyPairGenerator.GenerateRsaKeyPair(keySize);
            byte[] publicKeyContent = GetPublicKey(rsaKeyPair.Public);
            byte[] privateKeyContent = GetPrivateKey(rsaKeyPair.Private);       

            int privateKeyLength = GetKeyLength(rsaKeyPair.Private);
            int publicKeyLength = GetKeyLength(rsaKeyPair.Public);

            var publicKey = new RsaKey(publicKeyContent, AsymmetricKeyType.Public, publicKeyLength);
            var privateKey = new RsaKey(privateKeyContent, AsymmetricKeyType.Private, privateKeyLength);

            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((RsaKeyParameters) key).Modulus.BitLength;

        public RsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            AsymmetricKeyParameter key = CreateKey(content, keyType);
            int keyLength = GetKeyLength(key);
            return new RsaKey(content, keyType, keyLength);
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            RsaKeyParameters privateKey;
            RsaKeyParameters publicKey;

            try
            {
                publicKey = GetKeyParameter<RsaKeyParameters>(keyPair.PublicKey?.Content, AsymmetricKeyType.Public);
                privateKey = GetKeyParameter<RsaKeyParameters>(keyPair.PrivateKey?.Content, AsymmetricKeyType.Private);

            }
            catch (CryptographicException)
            {
                return false;
            }

            return privateKey.Modulus.Equals(publicKey.Modulus);
        }
    }
}