using Core.Interfaces;
using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class ReadKeyFromTextFileCommandHandler : ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>>
    {
        private readonly FileWrapper file;

        public ReadKeyFromTextFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Execute(ReadFromTextFileCommand<IAsymmetricKey> createKeyCommand)
        {
            createKeyCommand.ContentFromFile = file.ReadAllText(createKeyCommand.FilePath);
        }
    }
}