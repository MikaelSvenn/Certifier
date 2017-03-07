using Core.Interfaces;

namespace Core.Model
{
    public class AsymmetricKeyPair : IAsymmetricKeyPair
    {
        public AsymmetricKeyPair(IAsymmetricKey privateKey, IAsymmetricKey publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public IAsymmetricKey PrivateKey { get; }
        public IAsymmetricKey PublicKey { get; }
        public int KeyLengthInBits => PrivateKey.KeySize;
        public string Password { get; set; }
        public bool HasPassword => !string.IsNullOrEmpty(Password);
    }
}