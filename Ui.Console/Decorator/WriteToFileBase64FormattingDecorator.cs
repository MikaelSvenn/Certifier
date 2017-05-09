using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class WriteToFileBase64FormattingDecorator<T> : ICommandHandler<T> where T : WriteToTextFileCommand<Signature>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly Base64Wrapper base64;

        public WriteToFileBase64FormattingDecorator(ICommandHandler<T> decoratedCommandHandler, Base64Wrapper base64)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.base64 = base64;
        }

        public void Execute(T command)
        {
            command.FileContent = base64.ToBase64String(command.Result.Content);
            decoratedCommandHandler.Execute(command);
        }
    }
}