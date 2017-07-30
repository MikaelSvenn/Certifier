using Core.Model;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class FileCommandProvider
    {
        public ReadKeyFromFileCommand GetReadPrivateKeyFromFileCommand(string targetPath, string password = "") => new ReadKeyFromFileCommand
        {
            FilePath = targetPath,
            Password = password,
            IsPrivateKey = true
        };

        public ReadKeyFromFileCommand GetReadPublicKeyFromFileCommand(string filePath) => new ReadKeyFromFileCommand
        {
            FilePath = filePath
        };

        public ReadFileCommand<T> GetReadFileCommand<T>(string filePath) => new ReadFileCommand<T>()
        {
            FilePath = filePath
        };

        public WriteFileCommand<T> GetWriteToFileCommand<T>(T input, string output, ContentType contentType = ContentType.NotSpecified, EncryptionType encryptionType = EncryptionType.None, string password = "") => new WriteFileCommand<T>
        {
            Out = input,
            FilePath = output,
            ContentType = contentType,
            EncryptionType = encryptionType,
            Password = password
        };

        public WriteToStdOutCommand<T> GetWriteToStdOutCommand<T>(T content) => new WriteToStdOutCommand<T>
        {
            Out = content
        };
    }
}