using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class WriteToStdOutBase64FormattingDecorator<T> : ICommandHandler<T> where T : WriteToStdOutCommand<Signature>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly Base64Wrapper base64;

        public WriteToStdOutBase64FormattingDecorator(ICommandHandler<T> decoratedCommandHandler, Base64Wrapper base64)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.base64 = base64;
        }

        public void Execute(T command)
        {
            command.ContentToStdOut = base64.ToBase64String(command.Out.Content);
            decoratedCommandHandler.Execute(command);
        }
    }
}