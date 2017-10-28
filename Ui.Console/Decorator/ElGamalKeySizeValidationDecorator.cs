using System;
using System.Linq;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class ElGamalKeySizeValidationDecorator<T> : ICommandHandler<T> where T : CreateKeyCommand<ElGamalKey>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        public ElGamalKeySizeValidationDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Execute(T command)
        {
            var validKeySizes = new[] {2048, 3072, 4096, 6144, 8192};
            if (!validKeySizes.Contains(command.KeySize))
            {
                throw new ArgumentException("ElGamal key size can either be 2048, 3072, 4096, 6144 or 8192 bits.");
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}