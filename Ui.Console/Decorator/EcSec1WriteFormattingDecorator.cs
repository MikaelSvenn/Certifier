using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EcSec1WriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IEcKeyProvider ecKeyProvider;

        public EcSec1WriteFormattingDecorator(ICommandHandler<T> decoratedCommand, IEcKeyProvider ecKeyProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.ecKeyProvider = ecKeyProvider;
        }

        public void Execute(T command)
        {
            if (command.Out.CipherType == CipherType.Ec && command.Out.IsPrivateKey && command.ContentType == ContentType.Sec1)
            {
                command.Out = ecKeyProvider.GetPkcs8PrivateKeyAsSec1((IEcKey)command.Out);
            }
            
            decoratedCommand.Execute(command);
        }
    }
}