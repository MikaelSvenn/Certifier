using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class FileCommandProvider
    {
        public WriteToFileCommand<IAsymmetricKey> GetWriteKeyToFileCommand(IAsymmetricKey key, string targetPath)
        {
            return new WriteToFileCommand<IAsymmetricKey>
            {
                Result = key,
                FilePath = targetPath
            };
        }

        public ReadKeyFromFileCommand GetReadKeyFromFileCommand(string targetPath, string password = "")
        {
            return new ReadKeyFromFileCommand
            {
                FilePath = targetPath,
                Password = password
            };
        }

        public ReadFromFileCommand GetReadFromFileCommand(string filePath)
        {
            return new ReadFromFileCommand
            {
                FilePath = filePath
            };
        }

        public WriteToFileCommand<Signature> GetWriteSignatureToFileCommand(Signature signature, string filePath)
        {
            return new WriteToFileCommand<Signature>
            {
                Result = signature,
                FilePath = $"{filePath}.signature"
            };
        }
    }
}