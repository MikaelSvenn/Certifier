using System;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class ReadKeyFromFilePathValidationDecorator<T> : ICommandHandler<T> where T : ReadKeyFromFileCommand
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;

        public ReadKeyFromFilePathValidationDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Execute(T command)
        {
            if (string.IsNullOrWhiteSpace(command.FilePath))
            {
                var keyType = command.IsPrivateKey ? "Private" : "Public";
                throw new ArgumentException($"{keyType} key file or path is required.");
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}