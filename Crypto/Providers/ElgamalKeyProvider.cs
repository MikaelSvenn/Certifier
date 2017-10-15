using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Crypto.Providers
{
    public class ElGamalKeyProvider : BCKeyProvider, IKeyProvider<ElGamalKey>
    {
        private readonly AsymmetricKeyPairGenerator keyPairGenerator;
        public ElGamalKeyProvider(AsymmetricKeyPairGenerator keyPairGenerator)
        {
            this.keyPairGenerator = keyPairGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize, BigInteger prime, BigInteger generator)
        {
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateElGamalKeyPair(keySize, prime, generator);
            byte[] publicKeyContent = GetPublicKey(keyPair.Public);
            byte[] privateKeyContent = GetPrivateKey(keyPair.Private);            

            var publicKey = new ElGamalKey(publicKeyContent, AsymmetricKeyType.Public, GetKeyLength(keyPair.Public));
            var privateKey = new ElGamalKey(privateKeyContent, AsymmetricKeyType.Private, GetKeyLength(keyPair.Private));
            
            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((ElGamalKeyParameters) key).Parameters.P.BitLength;

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            return CreateKeyPair(keySize, null, null);
        }

        public ElGamalKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            AsymmetricKeyParameter key = CreateKey(content, keyType);
            int keyLength = GetKeyLength(key);
            return new ElGamalKey(content, keyType, keyLength);
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            ElGamalPrivateKeyParameters privateKey;
            ElGamalPublicKeyParameters publicKey;

            try
            {
                publicKey = GetKeyParameter<ElGamalPublicKeyParameters>(keyPair.PublicKey?.Content, AsymmetricKeyType.Public);
                privateKey = GetKeyParameter<ElGamalPrivateKeyParameters>(keyPair.PrivateKey?.Content, AsymmetricKeyType.Private);
            }
            catch (CryptographicException)
            {
                return false;
            }

            return privateKey.Parameters.G.BitCount > 0 &&
                   privateKey.Parameters.P.BitCount > 0 &&
                   privateKey.Parameters.P.Equals(publicKey.Parameters.P) &&
                   privateKey.Parameters.G.Equals(publicKey.Parameters.G) &&
                   publicKey.Y.Equals(publicKey.Parameters.G.ModPow(privateKey.X, publicKey.Parameters.P).Mod(publicKey.Parameters.P));
        }
    }
}