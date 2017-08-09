﻿using System;
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
    public class RsaKeyProvider : IKeyProvider<RsaKey>
    {
        private readonly AsymmetricKeyPairGenerator asymmetricKeyPairGenerator;

        public RsaKeyProvider(AsymmetricKeyPairGenerator asymmetricKeyPairGenerator)
        {
            this.asymmetricKeyPairGenerator = asymmetricKeyPairGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            AsymmetricCipherKeyPair rsaKeyPair = asymmetricKeyPairGenerator.GenerateRsaKeyPair(keySize);
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

        private int GetKeyLength(AsymmetricKeyParameter key) => ((RsaKeyParameters) key).Modulus.BitLength;

        public RsaKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {
            if (keyType == AsymmetricKeyType.Encrypted)
            {
                throw new InvalidOperationException("Key must be decrypted prior to usage.");
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

            int keyLength = GetKeyLength(key);
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