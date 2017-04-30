using System;
using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class WriteKeyToFilePathValidationDecorator: ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>
    {
        private readonly ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommandHandler;

        public WriteKeyToFilePathValidationDecorator(ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Excecute(WriteToTextFileCommand<IAsymmetricKey> command)
        {
            if (string.IsNullOrWhiteSpace(command.Destination))
            {
                var keyType = command.Content.IsPrivateKey ? "Private" : "Public";
                throw new ArgumentException($"{keyType} key file or path is required.");
            }

            decoratedCommandHandler.Excecute(command);
        }
    }
}