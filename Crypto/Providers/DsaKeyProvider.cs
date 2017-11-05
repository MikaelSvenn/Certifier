using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Crypto.Providers
{
    public class DsaKeyProvider : BCKeyProvider, IDsaKeyProvider
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

            var publicKey = new DsaKey(publicKeyContent, AsymmetricKeyType.Public, GetKeyLength(keyPair.Public));
            var privateKey = new DsaKey(privateKeyContent, AsymmetricKeyType.Private, GetKeyLength(keyPair.Private));
            
            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((DsaKeyParameters) key).Parameters.P.BitLength;

        public DsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            AsymmetricKeyParameter key = CreateKey(content, keyType);
            int keyLength = GetKeyLength(key);
            return new DsaKey(content, keyType, keyLength);
        }

        public DsaKey GetPublicKey(byte[] p, byte[] q, byte[] g, byte[] y)
        {
            var pValue = new BigInteger(p);
            var qValue = new BigInteger(q);
            var gValue = new BigInteger(g);
            var yValue = new BigInteger(y);
            
            var dsaParameters = new DsaParameters(pValue, qValue, gValue);
            var dsaPublicKeyParameters = new DsaPublicKeyParameters(yValue, dsaParameters);
            
            byte[] publicKeyContent = GetPublicKey(dsaPublicKeyParameters);
            int keySize = GetKeyLength(dsaPublicKeyParameters);
            
            return new DsaKey(publicKeyContent, AsymmetricKeyType.Public, keySize);
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