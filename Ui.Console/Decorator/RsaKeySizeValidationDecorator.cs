using System;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class RsaKeySizeValidationDecorator<T> : ICommandHandler<T> where T : CreateRsaKeyCommand
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public RsaKeySizeValidationDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Execute(T createKeyCommand)
        {
            if (createKeyCommand.KeySize < 2048)
            {
                throw new ArgumentException("RSA key size too small. At least 2048 bit keys are required.");
            }

            decoratedCommand.Execute(createKeyCommand);
        }
    }
}