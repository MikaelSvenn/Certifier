using System;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class VerifyKeyTypeValidationDecorator<T> : ICommandHandler<T> where T : IVerifyKeyPairCommand {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        public VerifyKeyTypeValidationDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Execute(T command)
        {
            if (command.PrivateKey.CipherType != command.PublicKey.CipherType)
            {
                throw new InvalidOperationException($"Private key is (${command.PrivateKey.CipherType}) is not the same type as public key ${command.PublicKey.CipherType}");
            }
            
            decoratedCommandHandler.Execute(command);
        }
    }
}