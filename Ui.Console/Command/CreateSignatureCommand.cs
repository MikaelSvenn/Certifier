using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public class CreateSignatureCommand : ICommandWithResult<Signature>
    {
        public IAsymmetricKey PrivateKey { get; set; }
        public string Password { get; set; }
        public byte[] ContentToSign { get; set; }
        public Signature Result { get; set; }
    }
}