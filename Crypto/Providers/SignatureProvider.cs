﻿using Core.Interfaces;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using SignatureModel = Core.Model.Signature;

namespace Crypto.Providers
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly SignatureAlgorithmGenerator signatureAlgorithmGenerator;


        public SignatureProvider(SignatureAlgorithmGenerator signatureAlgorithmGenerator)
        {
            this.signatureAlgorithmGenerator = signatureAlgorithmGenerator;
        }

        public SignatureModel CreateSignature(IAsymmetricKey asymmetricKey, byte[] content)
        {
            ISigner signer = signatureAlgorithmGenerator.GetForSigning(asymmetricKey);
            signer.BlockUpdate(content, 0, content.Length);
            var signature = signer.GenerateSignature();

            return new SignatureModel
            {
                Content = signature,
                SignedData = content
            };
        }

        public bool VerifySignature(IAsymmetricKey asymmetricKey, SignatureModel signature)
        {
            ISigner signer = signatureAlgorithmGenerator.GetForVerifyingSignature(asymmetricKey);
            signer.BlockUpdate(signature.SignedData, 0, signature.SignedData.Length);
            return signer.VerifySignature(signature.Content);
        }
    }
}