using System;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8PemReadFormattingDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedHandler;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;
        private readonly EncodingWrapper encoding;

        public Pkcs8PemReadFormattingDecorator(ICommandHandler<T> decoratedHandler, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider, EncodingWrapper encoding)
        {
            this.decoratedHandler = decoratedHandler;
            this.formattingProvider = formattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            decoratedHandler.Execute(command);
            string keyContent = encoding.GetString(command.FileContent);
            if (!keyContent.StartsWith("-----BEGIN", StringComparison.InvariantCulture))
            {
                return;
            }
            
            command.Result = formattingProvider.GetAsDer(keyContent);
            command.OriginalContentType = ContentType.Pem;
        }
    }
}