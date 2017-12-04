using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EcSec1PemWriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IPemFormattingProvider<IEcKey> formattingProvider;
        private readonly EncodingWrapper encoding;

        public EcSec1PemWriteFormattingDecorator(ICommandHandler<T> decoratedCommand, IPemFormattingProvider<IEcKey> formattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            if (command.Out.CipherType == CipherType.Ec && command.ContentType == ContentType.Sec1)
            {
                string keyContent = formattingProvider.GetAsPem((IEcKey) command.Out);
                command.FileContent = encoding.GetBytes(keyContent);
            }
            
            decoratedCommand.Execute(command);
        }
    }
}