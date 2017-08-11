using Core.Interfaces;

namespace Ui.Console.Command
{
    public class VerifyKeyPairCommand : IVerifyKeyPairCommand
    {
        public IAsymmetricKey PublicKey { get; set; }
        public IAsymmetricKey PrivateKey { get; set; }
    }
}