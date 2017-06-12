using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class VerifyRsaKeyPairCommandHandler : ICommandHandler<VerifyRsaKeyPairCommand>
    {
        private readonly IAsymmetricKeyProvider<RsaKey> rsaKeyProvider;
        public VerifyRsaKeyPairCommandHandler(IAsymmetricKeyProvider<RsaKey> rsaKeyProvider)
        {
            this.rsaKeyProvider = rsaKeyProvider;
        }

        public void Execute(VerifyRsaKeyPairCommand command)
        {
            bool isValidKeyPair = rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(command.PrivateKey, command.PublicKey));
            if (!isValidKeyPair)
            {
                throw new CryptographicException("The given key pair is not valid.");
            }
        }
    }
}