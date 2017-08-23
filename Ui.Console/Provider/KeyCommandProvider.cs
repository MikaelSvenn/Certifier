using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class KeyCommandProvider
    {
        public CreateKeyCommand<T> GetCreateKeyCommand<T>(int keySize, string curve = "") where T : IAsymmetricKey => new CreateKeyCommand<T>()
        {
            KeySize = keySize,
            Curve = curve
        };
        
        public IVerifyKeyPairCommand GetVerifyKeyPairCommand(IAsymmetricKey publicKey, IAsymmetricKey privateKey) => new VerifyKeyPairCommand
        {
            PrivateKey = privateKey,
            PublicKey = publicKey
        };
    }
}