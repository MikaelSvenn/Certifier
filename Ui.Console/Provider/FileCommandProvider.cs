using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class FileCommandProvider
    {
        public WriteToTextFileCommand<IAsymmetricKey> GetWriteKeyToTextFileCommand(IAsymmetricKey key, string targetPath)
        {
            return new WriteToTextFileCommand<IAsymmetricKey>
            {
                Result = key,
                FilePath = targetPath
            };
        }

        public ReadFromTextFileCommand<IAsymmetricKey> GetReadKeyFromTextFileCommand(string targetPath)
        {
            return new ReadFromTextFileCommand<IAsymmetricKey>
            {
                FilePath = targetPath
            };
        }
    }
}