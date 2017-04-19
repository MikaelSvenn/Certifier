using Core.Interfaces;
using Core.SystemWrappers;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class WriteToTextFileCommandHandler : ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>
    {
        private readonly FileWrapper file;

        public WriteToTextFileCommandHandler(FileWrapper file)
        {
            this.file = file;
        }

        public void Excecute(WriteToTextFileCommand<IAsymmetricKey> createKeyCommand)
        {
            file.WriteAllText(createKeyCommand.Destination, createKeyCommand.ToFile);
        }
    }
}