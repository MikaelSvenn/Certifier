using Core.Interfaces;

namespace Core.Model
{
    public class RsaKeyPair : IAsymmetricKey
    {
        public RsaKeyPair(byte[] privateKey, byte[] publicKey, int keyLength, AsymmetricKeyType type)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
            KeyLengthInBits = keyLength;
            KeyType = type;
        }

        public byte[] PrivateKey { get; }
        public byte[] PublicKey { get; }
        public int KeyLengthInBits { get; }
        public AsymmetricKeyType KeyType { get; }
        public bool IsEncryptedPrivateKey => KeyType == AsymmetricKeyType.RsaPkcs12;
        public string Password { get; set; }
        public bool HasPassword => !string.IsNullOrEmpty(Password);
    }
}