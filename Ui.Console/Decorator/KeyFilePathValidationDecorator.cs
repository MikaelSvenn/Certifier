using System;
using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class KeyFilePathValidationDecorator<T>: ICommandHandler<T> where T : FileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;

        public KeyFilePathValidationDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Execute(T command)
        {
            if (string.IsNullOrWhiteSpace(command.FilePath))
            {
                var keyType = command.Result.IsPrivateKey ? "Private" : "Public";
                throw new ArgumentException($"{keyType} key file or path is required.");
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}