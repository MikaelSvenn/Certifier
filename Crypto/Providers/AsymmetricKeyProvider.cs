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
        private readonly IKeyProvider<DsaKey> dsaKeyProvider;
        private readonly IEcKeyProvider ecKeyProvider;
        private readonly IElGamalKeyProvider elGamalKeyProvider;
        private readonly KeyInfoWrapper keyInfoWrapper;

        public AsymmetricKeyProvider(OidToCipherTypeMapper cipherTypeMapper, KeyInfoWrapper keyInfoWrapper, IKeyProvider<RsaKey> rsaKeyProvider, IKeyProvider<DsaKey> dsaKeyProvider, IEcKeyProvider ecKeyProvider, IElGamalKeyProvider elGamalKeyProvider)
        {
            this.cipherTypeMapper = cipherTypeMapper;
            this.rsaKeyProvider = rsaKeyProvider;
            this.dsaKeyProvider = dsaKeyProvider;
            this.ecKeyProvider = ecKeyProvider;
            this.elGamalKeyProvider = elGamalKeyProvider;
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
            
            return GetKey(keyContent, publicKeyInfo.AlgorithmID.Algorithm.Id, AsymmetricKeyType.Public);
        }

        private IAsymmetricKey GetKey(byte[] keyContent, string algorithmId, AsymmetricKeyType keyType)
        {
            var cipherType = cipherTypeMapper.MapOidToCipherType(algorithmId);
            switch (cipherType)
            {
                    case CipherType.Rsa:
                        return rsaKeyProvider.GetKey(keyContent, keyType);
                    case CipherType.Dsa:
                        return dsaKeyProvider.GetKey(keyContent, keyType);
                    case CipherType.Ec:
                        return ecKeyProvider.GetKey(keyContent, keyType);
                    case CipherType.ElGamal:
                        return elGamalKeyProvider.GetKey(keyContent, keyType);
                    default:
                        throw new ArgumentException("Key type not supported or key is corrupted.");        
            }
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

            return GetKey(keyContent, privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id, AsymmetricKeyType.Private);
        }

        public IAsymmetricKey GetEncryptedPrivateKey(byte[] keyContent)
        {
            var encrpytedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(keyContent));
            var cipherType = cipherTypeMapper.MapOidToCipherType(encrpytedPrivateKeyInfo.EncryptionAlgorithm.Algorithm.Id);

            return new EncryptedKey(keyContent, cipherType);
        }
    }
}