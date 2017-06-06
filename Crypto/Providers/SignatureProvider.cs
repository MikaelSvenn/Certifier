using System;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Wrappers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SignatureModel = Core.Model.Signature;

namespace Crypto.Providers
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly SignatureAlgorithmIdentifierMapper algorithmIdentifierMapper;
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly SignerUtilitiesWrapper signerUtilitiesWrapper;

        public SignatureProvider(SignatureAlgorithmIdentifierMapper algorithmIdentifierMapper, SecureRandomGenerator secureRandomGenerator, SignerUtilitiesWrapper signerUtilitiesWrapper)
        {
            this.algorithmIdentifierMapper = algorithmIdentifierMapper;
            this.secureRandomGenerator = secureRandomGenerator;
            this.signerUtilitiesWrapper = signerUtilitiesWrapper;
        }

        public SignatureModel CreateSignature(IAsymmetricKey privateKey, byte[] content)
        {
            if (privateKey.IsEncrypted)
            {
                throw new InvalidOperationException("Key must be decrypted before signing.");
            }

            string signatureAlgorithm = algorithmIdentifierMapper.MapToAlgorithmIdentifier(privateKey.CipherType);
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);

            AsymmetricKeyParameter keyParameter = PrivateKeyFactory.CreateKey(privateKey.Content);
            var cipherParameters = new ParametersWithRandom(keyParameter, secureRandomGenerator.Generator);

            signer.Init(true, cipherParameters);
            signer.BlockUpdate(content, 0, content.Length);
            byte[] signature = signer.GenerateSignature();

            return new SignatureModel
            {
                Content = signature,
                SignedData = content
            };
        }

        public bool VerifySignature(IAsymmetricKey publicKey, SignatureModel signature)
        {
            string signatureAlgorithm = algorithmIdentifierMapper.MapToAlgorithmIdentifier(publicKey.CipherType);
            var signer = signerUtilitiesWrapper.GetSigner(signatureAlgorithm);
            var keyParameters = PublicKeyFactory.CreateKey(publicKey.Content);

            signer.Init(false, keyParameters);
            signer.BlockUpdate(signature.SignedData, 0, signature.SignedData.Length);
            
            try
            {
                return signer.VerifySignature(signature.Content);
            }
            catch (DataLengthException)
            {
                return false;
            }
        }
    }
}