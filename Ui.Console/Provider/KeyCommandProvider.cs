using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class KeyCommandProvider
    {
        public ICreateAsymmetricKeyCommand GetCreateKeyCommand(int keySize) => new CreateRsaKeyCommand
        {
            KeySize = keySize
        };

        public IVerifyKeyPairCommand GetVerifyKeyPairCommand(IAsymmetricKey publicKey, IAsymmetricKey privateKey) => new VerifyRsaKeyPairCommand
        {
            PrivateKey = privateKey,
            PublicKey = publicKey
        };
    }
}