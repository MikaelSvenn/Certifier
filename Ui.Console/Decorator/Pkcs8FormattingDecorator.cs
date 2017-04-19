using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8FormattingDecorator : ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>
    {
        private readonly ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;

        public Pkcs8FormattingDecorator(ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
        }

        public void Excecute(WriteToTextFileCommand<IAsymmetricKey> writeToFileCommand)
        {
            writeToFileCommand.ToFile = formattingProvider.GetAsPem(writeToFileCommand.Content);
            decoratedCommand.Excecute(writeToFileCommand);
        }
    }
}