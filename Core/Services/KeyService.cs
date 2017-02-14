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
            return keyProvider.CreateAsymmetricKeyPair(password, keySizeInBits);
        }
    }
}