using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteToTextFileCommandHandler<T> : ICommandHandler<WriteToTextFileCommand<T>>
    {
        private readonly FileWrapper file;

        public WriteToTextFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(WriteToTextFileCommand<T> createKeyCommand)
        {
            file.WriteAllText(createKeyCommand.FilePath, createKeyCommand.FileContent);
        }
    }
}