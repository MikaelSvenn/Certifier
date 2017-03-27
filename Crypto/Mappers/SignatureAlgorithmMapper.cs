using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Crypto.Mappers
{
    public class SignatureAlgorithmMapper
    {
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly Dictionary<AsymmetricKeyType, string> bouncyCastleSignatureAlgorithms;

        public SignatureAlgorithmMapper(SecureRandomGenerator secureRandomGenerator)
        {
            this.secureRandomGenerator = secureRandomGenerator;
            bouncyCastleSignatureAlgorithms = new Dictionary<AsymmetricKeyType, string>
            {
                {AsymmetricKeyType.Public, "SHA-512withRSAandMGF1"},
                {AsymmetricKeyType.Private, "SHA-512withRSAandMGF1"},
                {AsymmetricKeyType.Encrypted, "SHA-512withRSAandMGF1"}
            };
        }

        public ISigner GetForSigning(IAsymmetricKey key, string password = "")
        {
            AsymmetricKeyParameter keyParameter;
            if (key.IsEncrypted)
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Private key password is required.");
                }

                keyParameter = PrivateKeyFactory.DecryptKey(password.ToCharArray(), key.Content);
            }
            else
            {
                keyParameter = PrivateKeyFactory.CreateKey(key.Content);
            }

            var signatureAlgorithm = bouncyCastleSignatureAlgorithms[key.KeyType];
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);

            var cipherParameters = new ParametersWithRandom(keyParameter, secureRandomGenerator.Generator);
            signer.Init(true, cipherParameters);

            return signer;
        }

        public ISigner GetForVerifyingSignature(IAsymmetricKey key)
        {
            var keyParameters = PublicKeyFactory.CreateKey(key.Content);
            string signatureAlgorithm = bouncyCastleSignatureAlgorithms[key.KeyType];
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);
            signer.Init(false, keyParameters);

            return signer;
        }
    }
}