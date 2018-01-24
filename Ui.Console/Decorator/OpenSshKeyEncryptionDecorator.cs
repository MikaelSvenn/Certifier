using System;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class OpenSshKeyEncryptionDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKeyPair>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        
        public OpenSshKeyEncryptionDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }
        
        public void Execute(T command)
        {
            if (command.EncryptionType != EncryptionType.None && command.ContentType == ContentType.OpenSsh)
            {
                throw new InvalidOperationException("Encrypted ssh-ed25519 keys are not supported.");
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}