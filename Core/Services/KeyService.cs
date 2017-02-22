using System;
using Core.Interfaces;

namespace Core.Services
{
    public class KeyService : IKeyService
    {
        private readonly IKeyProvider keyProvider;

        public KeyService(IKeyProvider keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public IAsymmetricKey CreateAsymmetricKeyPair(string password, int keySizeInBits = 2048)
        {
            if (keySizeInBits < 2048)
            {
                throw new ArgumentException("Key size below 2048 bits is not allowed.");
            }

            IAsymmetricKey keyPair = keyProvider.CreateAsymmetricKeyPair(keySizeInBits);
            return keyPair;
        }
    }
}