using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class CreateDsaKeyCommandHandler : ICommandHandler<CreateDsaKeyCommand>
    {
        private readonly IKeyProvider<DsaKey> keyProvider;
        public CreateDsaKeyCommandHandler(IKeyProvider<DsaKey> keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public void Execute(CreateDsaKeyCommand command)
        {
            command.Result = keyProvider.CreateKeyPair(command.KeySize);
        }
    }
}