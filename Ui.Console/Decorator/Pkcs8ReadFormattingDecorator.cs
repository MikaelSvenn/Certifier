using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8ReadFormattingDecorator : ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>>
    {
        private readonly ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;

        public Pkcs8ReadFormattingDecorator(ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
        }

        public void Execute(ReadFromTextFileCommand<IAsymmetricKey> readFromFileCommand)
        {
            decoratedCommand.Execute(readFromFileCommand);
            readFromFileCommand.Result = formattingProvider.GetAsDer(readFromFileCommand.ContentFromFile);
        }
    }
}