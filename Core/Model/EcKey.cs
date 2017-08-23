using Core.Interfaces;

namespace Core.Model
{
    public class EcKey : IAsymmetricKey {
        public EcKey(byte[] content, AsymmetricKeyType keyType, int keyLength)
        {
            Content = content;
            KeyType = keyType;
            KeySize = keyLength;
            CipherType = CipherType.Ec;
        }

        public byte[] Content { get; }
        public CipherType CipherType { get; }
        public AsymmetricKeyType KeyType { get; }
        public int KeySize { get; }
        public bool IsEncrypted => KeyType == AsymmetricKeyType.Encrypted;
        public bool IsPrivateKey => KeyType == AsymmetricKeyType.Private;
    }
}