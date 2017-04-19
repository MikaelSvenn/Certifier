using System;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EncryptionPasswordValidationDecorator<T> : ICommandHandler<T> where T : ICreateAsymmetricKeyCommand
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public EncryptionPasswordValidationDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Excecute(T createKeyCommand)
        {
            if (createKeyCommand.EncryptionType != KeyEncryptionType.None && string.IsNullOrEmpty(createKeyCommand.Password))
            {
                throw new ArgumentException("Password is required for encryption.");
            }

            decoratedCommand.Excecute(createKeyCommand);
        }
    }
}