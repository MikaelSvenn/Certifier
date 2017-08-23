using System;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class DsaKeySizeValidationDecorator<T> : ICommandHandler<T> where T : CreateKeyCommand<DsaKey>
    {
        private readonly ICommandHandler<T> decoratedHandler;
        public DsaKeySizeValidationDecorator(ICommandHandler<T> decoratedHandler)
        {
            this.decoratedHandler = decoratedHandler;
        }

        public void Execute(T command)
        {
            if (command.KeySize != 2048 && command.KeySize != 3072)
            {
                throw new ArgumentException("DSA key size can only be 2048 bit or 3072 bit.");
            }
            
            decoratedHandler.Execute(command);
        }
    }
}