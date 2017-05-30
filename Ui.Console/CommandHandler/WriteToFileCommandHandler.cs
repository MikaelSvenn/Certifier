using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteToFileCommandHandler<T> : ICommandHandler<WriteFileCommand<T>>
    {
        private readonly FileWrapper file;

        public WriteToFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(WriteFileCommand<T> createKeyCommand)
        {
            file.WriteAllBytes(createKeyCommand.FilePath, createKeyCommand.FileContent);
        }
    }
}