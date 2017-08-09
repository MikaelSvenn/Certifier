using System;
using System.IO;
using System.Security.Cryptography;
using Core.Model;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Crypto.Providers
{
    public class BCKeyProvider
    {        
        protected AsymmetricKeyParameter CreateKey(byte[] content, AsymmetricKeyType keyType)
        {
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

            return key;
        }

        protected byte[] GetPublicKey(AsymmetricKeyParameter key)
        {
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key);
            byte[] publicKeyContent = publicKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            return publicKeyContent;
        }

        protected byte[] GetPrivateKey(AsymmetricKeyParameter key)
        {
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(key);
            byte[] privateKeyContent = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            return privateKeyContent;
        }

        protected T GetKeyParameter<T>(byte[] keyContent, AsymmetricKeyType keyType) where T : AsymmetricKeyParameter
        {
            try
            {
                return (T) CreateKey(keyContent, keyType);
            }
            catch (Exception exception) when (exception is ArgumentNullException ||
                                              exception is IOException ||
                                              exception is ArgumentException ||
                                              exception is SecurityUtilityException ||
                                              exception is NullReferenceException ||
                                              exception is InvalidCastException)
            {
                throw new CryptographicException();
            }
        }
    }
}