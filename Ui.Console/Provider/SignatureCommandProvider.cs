using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class SignatureCommandProvider
    {
        public CreateSignatureCommand GetCreateSignatureCommand(IAsymmetricKey privateKey, byte[] content) => new CreateSignatureCommand
        {
            PrivateKey = privateKey,
            ContentToSign = content
        };

        public VerifySignatureCommand GetVerifySignatureCommand(IAsymmetricKey publicKey, byte[] signedContent, byte[] signature) => new VerifySignatureCommand
        {
            PublicKey = publicKey,
            Signature = new Signature
            {
                Content = signature,
                SignedData = signedContent
            }
        };
    }   
}