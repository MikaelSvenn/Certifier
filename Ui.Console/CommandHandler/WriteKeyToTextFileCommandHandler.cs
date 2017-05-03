using Core.Interfaces;
using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteKeyToTextFileCommandHandler : ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>
    {
        private readonly FileWrapper file;

        public WriteKeyToTextFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(WriteToTextFileCommand<IAsymmetricKey> createKeyCommand)
        {
            file.WriteAllText(createKeyCommand.Destination, createKeyCommand.ContentToFile);
        }
    }
}