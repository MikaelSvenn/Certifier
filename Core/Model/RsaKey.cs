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
            
            CipherType = CipherType.Rsa;
        }

        public byte[] Content { get; }
        public AsymmetricKeyType KeyType { get; }
        public CipherType CipherType { get; }
        public int KeySize { get; }
        public bool IsEncrypted => KeyType == AsymmetricKeyType.Encrypted;
        public bool IsPrivateKey => KeyType == AsymmetricKeyType.Private || KeyType == AsymmetricKeyType.Encrypted;
    }
}