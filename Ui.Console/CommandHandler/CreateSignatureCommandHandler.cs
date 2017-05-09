using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class CreateSignatureCommandHandler : ICommandHandler<CreateSignatureCommand>
    {
        private readonly ISignatureProvider signatureProvider;

        public CreateSignatureCommandHandler(ISignatureProvider signatureProvider)
        {
            this.signatureProvider = signatureProvider;
        }

        public void Execute(CreateSignatureCommand command)
        {
            command.Result = signatureProvider.CreateSignature(command.PrivateKey, command.ContentToSign);
        }
    }
}