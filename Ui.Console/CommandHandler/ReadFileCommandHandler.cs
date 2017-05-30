using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class ReadFileCommandHandler<T> : ICommandHandler<ReadFileCommand<byte[]>>
    {
        private readonly FileWrapper file;

        public ReadFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(ReadFileCommand<byte[]> command)
        {
            command.Result = file.ReadAllBytes(command.FilePath);
        }
    }
}