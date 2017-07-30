using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public class ReadKeyFromFileCommand : ReadFileCommand<IAsymmetricKey>
    {
        public string Password { get; set; }
        public bool IsPrivateKey { get; set; }
        public EncryptionType OriginalEncryptionType { get; set; }        
        public ContentType OriginalContentType { get; set; }
    }
}