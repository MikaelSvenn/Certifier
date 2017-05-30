using Core.Interfaces;

namespace Ui.Console.Command
{
    public class ReadKeyFromFileCommand : ReadFileCommand<IAsymmetricKey>
    {
        public string Password { get; set; }
        public bool IsPrivateKey { get; set; }
    }
}