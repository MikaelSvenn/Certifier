using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class SignatureCommandProvider
    {
        public CreateSignatureCommand GetCreateSignatureCommand(IAsymmetricKey key, byte[] content)
        {
            return new CreateSignatureCommand
            {
                PrivateKey = key,
                ContentToSign = content
            };
        }
    }
}