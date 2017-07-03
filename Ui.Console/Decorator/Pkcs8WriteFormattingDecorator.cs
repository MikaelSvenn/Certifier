using Core.Interfaces;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Startup;

namespace Ui.Console.Decorator
{
    public class Pkcs8WriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;
        private readonly EncodingWrapper encoding;

        public Pkcs8WriteFormattingDecorator(ICommandHandler<T> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            if (command.ContentType == ContentType.Pem)
            {
                string pemFormatted = formattingProvider.GetAsPem(command.Out);
                command.FileContent = encoding.GetBytes(pemFormatted);
            }
            else
            {
                command.FileContent = command.Out.Content;
            }
            
            decoratedCommand.Execute(command);
        }
    }
}