using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class CreateElGamalKeyCommandHandler : ICommandHandler<CreateKeyCommand<ElGamalKey>>
    {
        private readonly IElGamalKeyProvider elGamalKeyProvider;
        private readonly ConsoleWrapper console;

        public CreateElGamalKeyCommandHandler(IElGamalKeyProvider elGamalKeyProvider, ConsoleWrapper console)
        {
            this.elGamalKeyProvider = elGamalKeyProvider;
            this.console = console;
        }

        public void Execute(CreateKeyCommand<ElGamalKey> command)
        {
            if (!command.UseRfc3526Prime)
            {
                console.WriteLine("NOTE: Depending on system performance, creating an ElGamal keypair may take from 20 minutes up to several hours.");
            }
            
            command.Result = elGamalKeyProvider.CreateKeyPair(command.KeySize, command.UseRfc3526Prime);
        }
    }
}