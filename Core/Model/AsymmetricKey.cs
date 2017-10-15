using Core.Interfaces;

namespace Core.Model
{
    public class AsymmetricKey : IAsymmetricKey
    {
        public AsymmetricKey(byte[] content, AsymmetricKeyType keyType, int keyLength, CipherType cipherType)
        {
            Content = content;
            KeyType = keyType;
            KeySize = keyLength;
            CipherType = cipherType;
        }
        
        public byte[] Content { get; }
        public CipherType CipherType { get; }
        public AsymmetricKeyType KeyType { get; }
        public int KeySize { get; }
        public bool IsEncrypted => KeyType == AsymmetricKeyType.Encrypted;
        public bool IsPrivateKey => KeyType == AsymmetricKeyType.Private;
    }
}