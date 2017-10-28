using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class KeyCommandProvider
    {
        public CreateKeyCommand<T> GetCreateKeyCommand<T>(int keySize, bool useRfc3526Prime = false) where T : IAsymmetricKey => new CreateKeyCommand<T>()
        {
            KeySize = keySize,
            UseRfc3526Prime = useRfc3526Prime
        };
        
        public CreateKeyCommand<IEcKey> GetCreateKeyCommand<T>(string curve = "") where T : IEcKey => new CreateKeyCommand<IEcKey>()
        {
            Curve = curve
        };
        
        public IVerifyKeyPairCommand GetVerifyKeyPairCommand(IAsymmetricKey publicKey, IAsymmetricKey privateKey) => new VerifyKeyPairCommand
        {
            PrivateKey = privateKey,
            PublicKey = publicKey
        };
    }
}