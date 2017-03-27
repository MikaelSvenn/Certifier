using System;
using Core.Interfaces;
using Core.Model;
using Crypto.Mappers;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Crypto.Providers
{
    public class AsymmetricKeyProvider
    {
        private readonly OidToCipherTypeMapper cipherTypeMapper;
        private readonly IAsymmetricKeyProvider<RsaKey> rsaKeyProvider;

        //TODO: Extension point for reading in DSA and EC keys - implement once EC and DSA key providers are done.
        public AsymmetricKeyProvider(OidToCipherTypeMapper cipherTypeMapper, IAsymmetricKeyProvider<RsaKey> rsaKeyProvider)
        {
            this.cipherTypeMapper = cipherTypeMapper;
            this.rsaKeyProvider = rsaKeyProvider;
        }

        public IAsymmetricKey GetPublicKey(byte[] keyContent)
        {
            var publicKeyInfo = SubjectPublicKeyInfo.GetInstance(keyContent);
            var cipherType = cipherTypeMapper.MapOidToCipherType(publicKeyInfo.AlgorithmID.Algorithm.Id);

            if (cipherType != CipherType.Rsa)
            {
                throw new ArgumentException($"Unsupported key type: {cipherType}");
            }

            return rsaKeyProvider.GetKey(keyContent, AsymmetricKeyType.Public);
        }

        public IAsymmetricKey GetPrivateKey(byte[] keyContent)
        {
            var privateKeyInfo = PrivateKeyInfo.GetInstance(keyContent);
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