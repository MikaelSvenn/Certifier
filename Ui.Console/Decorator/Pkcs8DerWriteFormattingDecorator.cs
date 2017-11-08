using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8DerWriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;

        public Pkcs8DerWriteFormattingDecorator(ICommandHandler<T> decoratedCommand)
        {
            this.decoratedCommand = decoratedCommand;
        }

        public void Execute(T command)
        {
            if (command.ContentType == ContentType.Der || command.ContentType == ContentType.NotSpecified)
            {
                command.FileContent = command.Out.Content;
            }
            
            decoratedCommand.Execute(command);
        }
    }
}