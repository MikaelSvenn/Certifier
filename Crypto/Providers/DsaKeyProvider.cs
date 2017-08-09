using System;
using System.IO;
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
    public class DsaKeyProvider : IKeyProvider<DsaKey>
    {
        private readonly AsymmetricKeyPairGenerator keyGenerator;

        public DsaKeyProvider(AsymmetricKeyPairGenerator keyGenerator)
        {
            this.keyGenerator = keyGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair keyPair = keyGenerator.GenerateDsaKeyPair(keySize);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            byte[] publicKeyContent = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            byte[] privateKeyContent = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();
            
            var publicKey = new DsaKey(publicKeyContent, AsymmetricKeyType.Public, GetKeyLength(keyPair.Private));
            var privateKey = new DsaKey(privateKeyContent, AsymmetricKeyType.Private, GetKeyLength(keyPair.Public));
            
            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((DsaKeyParameters) key).Parameters.P.BitLength;

        public DsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            throw new NotImplementedException();
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            DsaPrivateKeyParameters privateKey;
            DsaPublicKeyParameters publicKey;
            
            try
            {
                AsymmetricKeyParameter publicKeyContent = PublicKeyFactory.CreateKey(keyPair.PublicKey?.Content);
                AsymmetricKeyParameter privateKeyContent = PrivateKeyFactory.CreateKey(keyPair.PrivateKey?.Content);

                publicKey = (DsaPublicKeyParameters) publicKeyContent;
                privateKey = (DsaPrivateKeyParameters) privateKeyContent;
            }
            catch (Exception exception) when (exception is ArgumentNullException ||
                                              exception is IOException ||
                                              exception is ArgumentException ||
                                              exception is SecurityUtilityException ||
                                              exception is NullReferenceException ||
                                              exception is InvalidCastException)
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