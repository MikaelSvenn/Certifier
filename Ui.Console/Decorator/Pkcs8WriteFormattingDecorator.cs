using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8WriteFormattingDecorator : ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>
    {
        private readonly ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;

        public Pkcs8WriteFormattingDecorator(ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
        }

        public void Execute(WriteToTextFileCommand<IAsymmetricKey> writeToFileCommand)
        {
            writeToFileCommand.ContentToFile = formattingProvider.GetAsPem(writeToFileCommand.Content);
            decoratedCommand.Execute(writeToFileCommand);
        }
    }
}