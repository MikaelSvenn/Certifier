using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class WriteToFileCommandProvider
    {
        public WriteToTextFileCommand<IAsymmetricKey> GetWriteKeyToTextFileCommand(IAsymmetricKey key, string targetPath)
        {
            return new WriteToTextFileCommand<IAsymmetricKey>
            {
                Content = key,
                Destination = targetPath
            };
        }
    }
}