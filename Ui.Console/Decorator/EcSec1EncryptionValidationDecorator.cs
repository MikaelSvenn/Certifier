using System;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EcSec1EncryptionValidationDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public EcSec1EncryptionValidationDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Execute(T command)
        {
            if (command.ContentType == ContentType.Sec1 && command.EncryptionType != EncryptionType.None)
            {
                throw new InvalidOperationException("Encryption of SEC 1 formatted EC keys is not supported.");
            }
            
            decoratedCommand.Execute(command);
        }
    }
}