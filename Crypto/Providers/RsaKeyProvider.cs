using System;
using System.CodeDom;
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
    public class RsaKeyProvider : IAsymmetricKeyProvider<RsaKey>
    {
        private readonly RsaKeyPairGenerator rsaKeyPairGenerator;

        public RsaKeyProvider(RsaKeyPairGenerator rsaKeyPairGenerator)
        {
            this.rsaKeyPairGenerator = rsaKeyPairGenerator;
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

        public RsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            if (keyType == AsymmetricKeyType.Encrypted)
            {
                throw new InvalidOperationException("Unable to access encrypted key content. Key must be decrypted first.");
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
                throw new ArgumentException("Key type mismatch.");
            }

            var keyLength = GetKeyLength(key);
            return new RsaKey(content, keyType, keyLength);
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            RsaKeyParameters privateKey;
            RsaKeyParameters publicKey;

            try
            {
                AsymmetricKeyParameter publicKeyContent = PublicKeyFactory.CreateKey(keyPair.PublicKey?.Content);
                AsymmetricKeyParameter privateKeyContent = PrivateKeyFactory.CreateKey(keyPair.PrivateKey?.Content);

                publicKey = (RsaKeyParameters) publicKeyContent;
                privateKey = (RsaKeyParameters) privateKeyContent;
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

            return privateKey.Modulus.Equals(publicKey.Modulus);
        }
    }
}