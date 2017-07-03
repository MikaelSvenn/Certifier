using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class ReadKeyConversionDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedHandler;
        private readonly IAsymmetricKeyProvider asymmetricKeyProvider;
        
        public ReadKeyConversionDecorator(ICommandHandler<T> decoratedHandler, IAsymmetricKeyProvider asymmetricKeyProvider)
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

            if (command.IsPrivateKey)
            {
                command.Result = string.IsNullOrEmpty(command.Password) ? asymmetricKeyProvider.GetPrivateKey(command.FileContent) : asymmetricKeyProvider.GetEncryptedPrivateKey(command.FileContent);
                return;
            }

            command.Result = asymmetricKeyProvider.GetPublicKey(command.FileContent);
        }
    }
}