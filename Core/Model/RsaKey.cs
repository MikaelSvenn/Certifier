using Core.Interfaces;

namespace Core.Model
{
    public class RsaKey : IAsymmetricKey
    {
        public RsaKey(byte[] content, AsymmetricKeyType keyType, int keySize)
        {
            Content = content;
            KeyType = keyType;
            KeySize = keySize;
        }

        public byte[] Content { get; }
        public AsymmetricKeyType KeyType { get; }
        public int KeySize { get; }
        public bool IsEncrypted => KeyType == AsymmetricKeyType.RsaPkcs12;
    }
}