using Core.Interfaces;
using Crypto.Mappers;
using Org.BouncyCastle.Crypto;
using SignatureModel = Core.Model.Signature;

namespace Crypto.Providers
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly SignatureAlgorithmProvider signatureAlgorithmProvider;


        public SignatureProvider(SignatureAlgorithmProvider signatureAlgorithmProvider)
        {
            this.signatureAlgorithmProvider = signatureAlgorithmProvider;
        }

        public SignatureModel CreateSignature(IAsymmetricKey privateKey, byte[] content, string password = "")
        {
            ISigner signer = signatureAlgorithmProvider.GetForSigning(privateKey, password);
            signer.BlockUpdate(content, 0, content.Length);
            var signature = signer.GenerateSignature();

            return new SignatureModel
            {
                Content = signature,
                SignedData = content
            };
        }

        public bool VerifySignature(IAsymmetricKey publicKey, SignatureModel signature)
        {
            ISigner signer = signatureAlgorithmProvider.GetForVerifyingSignature(publicKey);
            signer.BlockUpdate(signature.SignedData, 0, signature.SignedData.Length);
            return signer.VerifySignature(signature.Content);
        }
    }
}