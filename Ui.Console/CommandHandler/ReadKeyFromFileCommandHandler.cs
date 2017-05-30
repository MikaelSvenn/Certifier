using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class ReadKeyFromFileCommandHandler : ICommandHandler<ReadKeyFromFileCommand>
    {
        private readonly FileWrapper file;

        public ReadKeyFromFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(ReadKeyFromFileCommand readKeyCommand)
        {
            readKeyCommand.FileContent = file.ReadAllBytes(readKeyCommand.FilePath);
        }
    }
}