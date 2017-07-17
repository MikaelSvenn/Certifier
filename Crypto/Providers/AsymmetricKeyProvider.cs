using System;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Mappers;
using Crypto.Wrappers;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Crypto.Providers
{
    public class AsymmetricKeyProvider : IAsymmetricKeyProvider
    {
        private readonly OidToCipherTypeMapper cipherTypeMapper;
        private readonly IKeyProvider<RsaKey> rsaKeyProvider;
        private readonly KeyInfoWrapper keyInfoWrapper;

        //TODO: Extension point for reading in DSA and EC keys - implement once EC and DSA key providers are done.
        public AsymmetricKeyProvider(OidToCipherTypeMapper cipherTypeMapper, IKeyProvider<RsaKey> rsaKeyProvider, KeyInfoWrapper keyInfoWrapper)
        {
            this.cipherTypeMapper = cipherTypeMapper;
            this.rsaKeyProvider = rsaKeyProvider;
            this.keyInfoWrapper = keyInfoWrapper;
        }

        public IAsymmetricKey GetPublicKey(byte[] keyContent)
        {
            SubjectPublicKeyInfo publicKeyInfo;
            try
            {
                publicKeyInfo = keyInfoWrapper.GetPublicKeyInfo(keyContent);
            }
            catch (ArgumentException)
            {
                throw new CryptographicException("Public key is not valid.");
            }
            
            var cipherType = cipherTypeMapper.MapOidToCipherType(publicKeyInfo.AlgorithmID.Algorithm.Id);
            if (cipherType != CipherType.Rsa)
            {
                throw new ArgumentException($"Unsupported key type: {cipherType}");
            }

            return rsaKeyProvider.GetKey(keyContent, AsymmetricKeyType.Public);
        }

        public IAsymmetricKey GetPrivateKey(byte[] keyContent)
        {
            PrivateKeyInfo privateKeyInfo;
            try
            {
                privateKeyInfo = keyInfoWrapper.GetPrivateKeyInfo(keyContent);
            }
            catch (Exception exception) when (exception is ArgumentException || 
                                              exception is InvalidCastException)
            {
                throw new CryptographicException("Private key is not valid.");
            }
            
            var cipherType = cipherTypeMapper.MapOidToCipherType(privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id);
            if (cipherType != CipherType.Rsa)
            {
                throw new ArgumentException($"Unsupported key type: {cipherType}");
            }

            return rsaKeyProvider.GetKey(keyContent, AsymmetricKeyType.Private);
        }

        public IAsymmetricKey GetEncryptedPrivateKey(byte[] keyContent)
        {
            var encrpytedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(keyContent));
            var cipherType = cipherTypeMapper.MapOidToCipherType(encrpytedPrivateKeyInfo.EncryptionAlgorithm.Algorithm.Id);

            return new EncryptedKey(keyContent, cipherType);
        }
    }
}