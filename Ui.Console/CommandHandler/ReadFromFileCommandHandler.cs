using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class ReadFromFileCommandHandler : ICommandHandler<ReadFromFileCommand>
    {
        private readonly FileWrapper file;

        public ReadFromFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(ReadFromFileCommand command)
        {
            command.Result = file.ReadAllBytes(command.FilePath);
        }
    }
}