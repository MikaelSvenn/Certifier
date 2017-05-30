using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteToStdOutCommandHandler<T> : ICommandHandler<WriteToStdOutCommand<T>>
    {
        private readonly ConsoleWrapper console;

        public WriteToStdOutCommandHandler(ConsoleWrapper console)
        {
            this.console = console;
        }

        public void Execute(WriteToStdOutCommand<T> command)
        {
            console.WriteLine(command.ContentToStdOut);
        }
    }
}