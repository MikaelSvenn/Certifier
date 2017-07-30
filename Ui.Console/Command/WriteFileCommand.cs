using Core.Model;
using Ui.Console.Startup;

namespace Ui.Console.Command
{
    public class WriteFileCommand<T> : ICommandWithOutput<T>
    {
        public byte[] FileContent { get; set; }
        public string FilePath { get; set; }
        public T Out { get; set; }
        public ContentType ContentType { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string Password { get; set; }
    }
}