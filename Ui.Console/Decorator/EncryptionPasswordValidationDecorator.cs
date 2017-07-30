using System;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EncryptionPasswordValidationDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public EncryptionPasswordValidationDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Execute(T writeToFileCommand)
        {
            if (writeToFileCommand.EncryptionType != EncryptionType.None && string.IsNullOrEmpty(writeToFileCommand.Password))
            {
                throw new ArgumentException("Password is required for encryption.");
            }

            decoratedCommand.Execute(writeToFileCommand);
        }
    }
}