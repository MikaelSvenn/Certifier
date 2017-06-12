using Core.Interfaces;

namespace Ui.Console.Command
{
    public class VerifyRsaKeyPairCommand : IVerifyKeyPairCommand
    {
        public IAsymmetricKey PublicKey { get; set; }
        public IAsymmetricKey PrivateKey { get; set; }
    }
}