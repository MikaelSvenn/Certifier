using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Crypto.Providers
{
    public class DsaKeyProvider : BCKeyProvider, IKeyProvider<DsaKey>
    {
        private readonly AsymmetricKeyPairGenerator keyGenerator;

        public DsaKeyProvider(AsymmetricKeyPairGenerator keyGenerator)
        {
            this.keyGenerator = keyGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair keyPair = keyGenerator.GenerateDsaKeyPair(keySize);
            byte[] publicKeyContent = GetPublicKey(keyPair.Public);
            byte[] privateKeyContent = GetPrivateKey(keyPair.Private);            

            var publicKey = new DsaKey(publicKeyContent, AsymmetricKeyType.Public, GetKeyLength(keyPair.Private));
            var privateKey = new DsaKey(privateKeyContent, AsymmetricKeyType.Private, GetKeyLength(keyPair.Public));
            
            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((DsaKeyParameters) key).Parameters.P.BitLength;

        public DsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            AsymmetricKeyParameter key = CreateKey(content, keyType);
            int keyLength = GetKeyLength(key);
            return new DsaKey(content, keyType, keyLength);
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            DsaPrivateKeyParameters privateKey;
            DsaPublicKeyParameters publicKey;

            try
            {
                publicKey = GetKeyParameter<DsaPublicKeyParameters>(keyPair.PublicKey?.Content, AsymmetricKeyType.Public);
                privateKey = GetKeyParameter<DsaPrivateKeyParameters>(keyPair.PrivateKey?.Content, AsymmetricKeyType.Private);

            }
            catch (CryptographicException)
            {
                return false;
            }

            return privateKey.X.BitCount > 0 &&
                   publicKey.Y.BitCount > 0 &&
                   privateKey.Parameters.Equals(publicKey.Parameters) &&
                   publicKey.Y.Equals(publicKey.Parameters.G.ModPow(privateKey.X, publicKey.Parameters.P));
        }
    }
}