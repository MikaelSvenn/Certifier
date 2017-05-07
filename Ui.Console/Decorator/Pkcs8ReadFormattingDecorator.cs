using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8ReadFormattingDecorator<T> : ICommandHandler<T> where T : ReadFromTextFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;

        public Pkcs8ReadFormattingDecorator(ICommandHandler<T> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
        }

        public void Execute(T command)
        {
            decoratedCommand.Execute(command);
            command.Result = formattingProvider.GetAsDer(command.FileContent);
        }
    }
}