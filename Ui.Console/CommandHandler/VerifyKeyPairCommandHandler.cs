using System;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

namespace Ui.Console.CommandHandler
{
    public class VerifyKeyPairCommandHandler : ICommandHandler<VerifyKeyPairCommand>
    {
        private readonly IKeyProvider<RsaKey> rsaKeyProvider;
        private readonly IKeyProvider<DsaKey> dsaKeyProvider;

        public VerifyKeyPairCommandHandler(IKeyProvider<RsaKey> rsaKeyProvider, IKeyProvider<DsaKey> dsaKeyProvider)
        {
            this.rsaKeyProvider = rsaKeyProvider;
            this.dsaKeyProvider = dsaKeyProvider;
        }

        public void Execute(VerifyKeyPairCommand command)
        {
            bool isValidKeyPair;
            switch (command.PrivateKey.CipherType)
            {
                    case CipherType.Rsa:
                        isValidKeyPair = rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(command.PrivateKey, command.PublicKey));
                        break;
                    case CipherType.Dsa:
                        isValidKeyPair = dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(command.PrivateKey, command.PublicKey));
                        break;
                    default:
                        throw new InvalidOperationException("Key type not supported.");
            }
            
            if (!isValidKeyPair)
            {
                throw new CryptographicException("The given key pair is not valid.");
            }
        }
    }
}