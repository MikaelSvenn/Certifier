using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class CreateEcKeyCommandHandler : ICommandHandler<CreateKeyCommand<IEcKey>>
    {
        private readonly IEcKeyProvider keyProvider;
        public CreateEcKeyCommandHandler(IEcKeyProvider keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public void Execute(CreateKeyCommand<IEcKey> command)
        {
            command.Result = keyProvider.CreateKeyPair(command.Curve);
        }
    }
}