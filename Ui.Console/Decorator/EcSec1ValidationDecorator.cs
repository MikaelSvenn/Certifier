using System;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class EcSec1ValidationDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public EcSec1ValidationDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Execute(T command)
        {
            if (command.Out.CipherType != CipherType.Ec && command.ContentType == ContentType.Sec1)
            {
                throw new InvalidOperationException("Only EC keys can be formatted in SECG SEC 1 compatible format.");
            }

            decoratedCommand.Execute(command);
        }
    }
}