using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class CreateDsaKeyCommandHandler : ICommandHandler<CreateKeyCommand<DsaKey>>
    {
        private readonly IKeyProvider<DsaKey> keyProvider;
        public CreateDsaKeyCommandHandler(IKeyProvider<DsaKey> keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public void Execute(CreateKeyCommand<DsaKey> command)
        {
            command.Result = keyProvider.CreateKeyPair(command.KeySize);
        }
    }
}