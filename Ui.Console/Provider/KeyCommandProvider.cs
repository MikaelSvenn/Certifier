using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.Provider
{
    public class KeyCommandProvider
    {
        public ICreateAsymmetricKeyCommand GetCreateKeyCommand(int keySize, KeyEncryptionType encryptionType, string password)
        {
            return new CreateRsaKeyCommand
            {
                KeySize = keySize,
                EncryptionType = encryptionType,
                Password = password
            };
        }

        public IVerifyKeyPairCommand GetVerifyKeyPairCommand(IAsymmetricKey publicKey, IAsymmetricKey privateKey)
        {
            return new VerifyRsaKeyPairCommand
            {
                PrivateKey = privateKey,
                PublicKey = publicKey
            };
        }
    }
}