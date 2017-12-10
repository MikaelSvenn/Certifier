using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class AesKeyDecryptionDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly IKeyEncryptionProvider keyEncryptionProvider;

        public AesKeyDecryptionDecorator(ICommandHandler<T> decoratedCommandHandler, IKeyEncryptionProvider keyEncryptionProvider)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.keyEncryptionProvider = keyEncryptionProvider;
        }
        
        public void Execute(T command)
        {
            decoratedCommandHandler.Execute(command);
            
            if (!command.Result.IsEncrypted || command.Result.CipherType != CipherType.AesEncrypted)
            {
                return;
            }

            command.Result = keyEncryptionProvider.DecryptPrivateKey(command.Result, command.Password);
            command.OriginalEncryptionType = EncryptionType.Aes;
        }
    }
}