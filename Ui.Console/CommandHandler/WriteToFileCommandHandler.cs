using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteToFileCommandHandler<T> : ICommandHandler<WriteToFileCommand<T>>
    {
        private readonly FileWrapper file;

        public WriteToFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(WriteToFileCommand<T> createKeyCommand)
        {
            file.WriteAllBytes(createKeyCommand.FilePath, createKeyCommand.FileContent);
        }
    }
}