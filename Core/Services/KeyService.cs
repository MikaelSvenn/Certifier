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

        public IAsymmetricKey CreateAsymmetricKeyPair(string password, int keySizeInBits = 4096)
        {
            if (keySizeInBits < 4096)
            {
                throw new ArgumentException("Key size below 4096 bits is not allowed.");
            }

            return keyProvider.CreateAsymmetricKeyPair(password, keySizeInBits);
        }
    }
}