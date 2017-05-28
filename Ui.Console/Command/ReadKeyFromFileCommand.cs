using Core.Interfaces;

namespace Ui.Console.Command
{
    public class ReadKeyFromFileCommand : FileCommand<IAsymmetricKey>
    {
        public string Password { get; set; }
        public byte[] FileContent { get; set; }
    }
}