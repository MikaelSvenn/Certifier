using System.Text;
using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Pkcs8WriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteToFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly IPkcsFormattingProvider<IAsymmetricKey> formattingProvider;

        public Pkcs8WriteFormattingDecorator(ICommandHandler<T> decoratedCommand, IPkcsFormattingProvider<IAsymmetricKey> formattingProvider)
        {
            this.decoratedCommand = decoratedCommand;
            this.formattingProvider = formattingProvider;
        }

        public void Execute(T command)
        {
            string pemFormatted = formattingProvider.GetAsPem(command.Result);
            command.FileContent = Encoding.UTF8.GetBytes(pemFormatted);
            decoratedCommand.Execute(command);
        }
    }
}