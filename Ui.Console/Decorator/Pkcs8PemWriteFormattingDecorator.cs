using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8PemWriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IPemFormattingProvider<IAsymmetricKey> formattingProvider;
        private readonly EncodingWrapper encoding;

        public Pkcs8PemWriteFormattingDecorator(ICommandHandler<T> decoratedCommand, IPemFormattingProvider<IAsymmetricKey> formattingProvider, EncodingWrapper encoding)
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
            
            decoratedCommand.Execute(command);
        }
    }
}