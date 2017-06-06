using System.Security.Cryptography;
using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class VerifySignatureCommandHandler : ICommandHandler<VerifySignatureCommand>
    {
        private readonly ISignatureProvider signatureProvider;
        public VerifySignatureCommandHandler(ISignatureProvider signatureProvider)
        {
            this.signatureProvider = signatureProvider;
        }

        public void Execute(VerifySignatureCommand command)
        {
            bool isValid = signatureProvider.VerifySignature(command.PublicKey, command.Signature);
            if (!isValid)
            {
                throw new CryptographicException("The provided signature is not valid for the given content.");
            }
        }
    }
}