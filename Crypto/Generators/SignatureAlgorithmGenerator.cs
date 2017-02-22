using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Crypto.Generators
{
    public class SignatureAlgorithmGenerator
    {
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly Dictionary<AsymmetricKeyType, string> bouncyCastleSignatureAlgorithms;

        public SignatureAlgorithmGenerator(SecureRandomGenerator secureRandomGenerator)
        {
            this.secureRandomGenerator = secureRandomGenerator;
            bouncyCastleSignatureAlgorithms = new Dictionary<AsymmetricKeyType, string>
            {
                {AsymmetricKeyType.Rsa, "SHA-512withRSAandMGF1"},
                {AsymmetricKeyType.RsaPkcs12, "SHA-512withRSAandMGF1"}
            };
        }

        public ISigner GetForSigning(IAsymmetricKey keyPair)
        {
            AsymmetricKeyParameter key;
            if (keyPair.IsEncryptedPrivateKey)
            {
                if (string.IsNullOrEmpty(keyPair.Password))
                {
                    throw new ArgumentException("Private key password is required.");
                }

                key = PrivateKeyFactory.DecryptKey(keyPair.Password.ToCharArray(), keyPair.PrivateKey);
            }
            else
            {
                key = PrivateKeyFactory.CreateKey(keyPair.PrivateKey);
            }

            var signatureAlgorithm = bouncyCastleSignatureAlgorithms[keyPair.KeyType];
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);

            var cipherParameters = new ParametersWithRandom(key, secureRandomGenerator.Generator);
            signer.Init(true, cipherParameters);

            return signer;
        }

        public ISigner GetForVerifyingSignature(IAsymmetricKey keyPair)
        {
            var key = PublicKeyFactory.CreateKey(keyPair.PublicKey);
            string signatureAlgorithm = bouncyCastleSignatureAlgorithms[keyPair.KeyType];
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);
            signer.Init(false, key);

            return signer;
        }
    }
}