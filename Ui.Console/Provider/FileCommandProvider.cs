using Core.Interfaces;
using Core.Model;
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

        public ReadFromTextFileCommand<IAsymmetricKey> GetReadKeyFromTextFileCommand(string targetPath, string password)
        {
            return new ReadFromTextFileCommand<IAsymmetricKey>
            {
                FilePath = targetPath,
                Password = password
            };
        }

        public ReadFromFileCommand GetReadFormFileCommand(string filePath)
        {
            return new ReadFromFileCommand
            {
                FilePath = filePath
            };
        }

        public WriteToTextFileCommand<Signature> GetWriteSignatureToTextFileCommand(Signature signature, string filePath)
        {
            return new WriteToTextFileCommand<Signature>
            {
                Result = signature,
                FilePath = $"{filePath}.signature"
            };
        }
    }
}