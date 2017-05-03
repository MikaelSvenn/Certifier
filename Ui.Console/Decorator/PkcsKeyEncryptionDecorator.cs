using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class PkcsKeyEncryptionDecorator<T> : ICommandHandler<T> where T : ICreateAsymmetricKeyCommand
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IKeyEncryptionProvider keyEncryptionProvider;

        public PkcsKeyEncryptionDecorator(ICommandHandler<T> decoratedCommandHandler, IKeyEncryptionProvider keyEncryptionProvider)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.keyEncryptionProvider = keyEncryptionProvider;
        }

        public void Execute(T createKeyCommand)
        {
            decoratedCommandHandler.Execute(createKeyCommand);

            if (createKeyCommand.EncryptionType != KeyEncryptionType.Pkcs)
            {
                return;
            }

            var keyPair = createKeyCommand.Result;
            var encryptedPrivateKey = keyEncryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, createKeyCommand.Password);
            createKeyCommand.Result = new AsymmetricKeyPair(encryptedPrivateKey, keyPair.PublicKey);
        }
    }
}