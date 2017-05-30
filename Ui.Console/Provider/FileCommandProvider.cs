using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class FileCommandProvider
    {
        public ReadKeyFromFileCommand GetReadPrivateKeyFromFileCommand(string targetPath, string password = "")
        {
            return new ReadKeyFromFileCommand
            {
                FilePath = targetPath,
                Password = password,
                IsPrivateKey = true
            };
        }

        public ReadFileCommand<T> GetReadFileCommand<T>(string filePath)
        {
            return new ReadFileCommand<T>()
            {
                FilePath = filePath
            };
        }

        public WriteFileCommand<T> GetWriteToFileCommand<T>(T input, string output)
        {
            return new WriteFileCommand<T>
            {
                Out = input,
                FilePath = output
            };
        }

        public WriteToStdOutCommand<T> GetWriteToStdOutCommand<T>(T result)
        {
            return new WriteToStdOutCommand<T>
            {
                Out = result
            };
        }
    }
}