using Core.Interfaces;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class KeyCommandProvider
    {
        public ICreateAsymmetricKeyCommand GetCreateKeyCommand<T>(int keySize) where T : ICreateAsymmetricKeyCommand, new() => new T {KeySize = keySize};
        public IVerifyKeyPairCommand GetVerifyKeyPairCommand(IAsymmetricKey publicKey, IAsymmetricKey privateKey) => new VerifyKeyPairCommand
        {
            PrivateKey = privateKey,
            PublicKey = publicKey
        };
    }
}