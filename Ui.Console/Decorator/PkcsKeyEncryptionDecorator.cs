using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class PkcsKeyEncryptionDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IKeyEncryptionProvider keyEncryptionProvider;

        public PkcsKeyEncryptionDecorator(ICommandHandler<T> decoratedCommandHandler, IKeyEncryptionProvider keyEncryptionProvider)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.keyEncryptionProvider = keyEncryptionProvider;
        }

        public void Execute(T writeToFileCommand)
        {
            if (writeToFileCommand.EncryptionType == EncryptionType.Pkcs)
            {
                writeToFileCommand.Out = keyEncryptionProvider.EncryptPrivateKey(writeToFileCommand.Out, writeToFileCommand.Password);
            }

            decoratedCommandHandler.Execute(writeToFileCommand);
        }
    }
}