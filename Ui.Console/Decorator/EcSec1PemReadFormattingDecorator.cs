using System;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EcSec1PemReadFormattingDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IPemFormattingProvider<IEcKey> pemFormatter;
        private readonly EncodingWrapper encoding;

        public EcSec1PemReadFormattingDecorator(ICommandHandler<T> decoratedCommandHandler, IPemFormattingProvider<IEcKey> pemFormatter, EncodingWrapper encoding)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.pemFormatter = pemFormatter;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            decoratedCommandHandler.Execute(command);

            if (command.Result != null)
            {
                return;
            }
            
            string keyContent = encoding.GetString(command.FileContent);
            if (!keyContent.StartsWith("-----BEGIN EC PRIVATE KEY", StringComparison.InvariantCulture))
            {
                return;
            }
            
            command.Result = pemFormatter.GetAsDer(keyContent);
            command.OriginalContentType = ContentType.Sec1;
        }
    }
}