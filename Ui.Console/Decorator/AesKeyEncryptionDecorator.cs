using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class AesKeyEncryptionDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IKeyEncryptionProvider keyEncryptionProvider;
        
        public AesKeyEncryptionDecorator(ICommandHandler<T> decoratedCommandHandler, IKeyEncryptionProvider keyEncryptionProvider)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.keyEncryptionProvider = keyEncryptionProvider;
        }
        
        public void Execute(T command)
        {
            if (command.EncryptionType == EncryptionType.Aes)
            {
                command.Out = keyEncryptionProvider.EncryptPrivateKey(command.Out, command.Password, EncryptionType.Aes);
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}