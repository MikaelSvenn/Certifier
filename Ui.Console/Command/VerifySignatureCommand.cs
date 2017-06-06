using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public class VerifySignatureCommand : ICommand
    {
        public IAsymmetricKey PublicKey { get; set; }
        public Signature Signature { get; set; }
    }
}