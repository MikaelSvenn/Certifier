using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class FileCommandProvider
    {
        public ReadKeyFromFileCommand GetReadPrivateKeyFromFileCommand(string filePath, string password = "") => new ReadKeyFromFileCommand
        {
            FilePath = filePath,
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

        public WriteFileCommand<IAsymmetricKey> GetWriteKeyToFileCommand(IAsymmetricKey input, string output, ContentType contentType, EncryptionType encryptionType = EncryptionType.None, string password = "")
        {
            if (input.IsPrivateKey && contentType == ContentType.Ssh2 || 
                input.IsPrivateKey && input.CipherType == CipherType.Ec && !((IEcKey)input).IsCurve25519 && contentType == ContentType.OpenSsh ||
                input.IsPrivateKey && input.CipherType != CipherType.Ec && contentType == ContentType.OpenSsh ||
                !input.IsPrivateKey && contentType == ContentType.Sec1)
            {
                contentType = ContentType.Pem;
            }

            return GetWriteToFileCommand(input, output, contentType, encryptionType, password);
        }

        public WriteToStdOutCommand<T> GetWriteToStdOutCommand<T>(T content) => new WriteToStdOutCommand<T>
        {
            Out = content
        };
    }
}