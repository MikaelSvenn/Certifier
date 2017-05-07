using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class PkcsKeyDecryptionDecorator<T> : ICommandHandler<T> where T : ReadFromTextFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IKeyEncryptionProvider keyEncryptionProvider;

        public PkcsKeyDecryptionDecorator(ICommandHandler<T> decoratedCommandHandler, IKeyEncryptionProvider keyEncryptionProvider)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.keyEncryptionProvider = keyEncryptionProvider;
        }

        public void Execute(T command)
        {
            decoratedCommandHandler.Execute(command);

            var isPkcsEncrypted = command.Result.CipherType == CipherType.Pkcs5Encrypted ||
                                  command.Result.CipherType == CipherType.Pkcs12Encrypted;

            if (!command.Result.IsEncrypted || !isPkcsEncrypted)
            {
                return;
            }

            command.Result = keyEncryptionProvider.DecryptPrivateKey(command.Result, command.Password);
        }
    }
}