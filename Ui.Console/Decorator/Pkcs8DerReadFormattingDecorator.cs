using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8DerReadFormattingDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedHandler;
        private readonly IAsymmetricKeyProvider asymmetricKeyProvider;
        
        public Pkcs8DerReadFormattingDecorator(ICommandHandler<T> decoratedHandler, IAsymmetricKeyProvider asymmetricKeyProvider)
        {
            this.decoratedHandler = decoratedHandler;
            this.asymmetricKeyProvider = asymmetricKeyProvider;
        }

        public void Execute(T command)
        {
            decoratedHandler.Execute(command);
            if (command.Result != null)
            {
                return;
            }

            command.OriginalContentType = ContentType.Der;
            if (command.IsPrivateKey)
            {
                try
                {
                    command.Result = asymmetricKeyProvider.GetPrivateKey(command.FileContent);
                }
                catch (CryptographicException)
                {
                    command.Result = asymmetricKeyProvider.GetEncryptedPrivateKey(command.FileContent);
                }
                
                return;
            }

            command.Result = asymmetricKeyProvider.GetPublicKey(command.FileContent);
        }
    }
}