using Core.Interfaces;

namespace Ui.Console.Command
{
    public interface IVerifyKeyPairCommand
    {
        IAsymmetricKey PublicKey { get; set; }
        IAsymmetricKey PrivateKey { get; set; }
    }
}