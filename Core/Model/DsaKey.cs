using Core.Interfaces;

namespace Core.Model {
    public class DsaKey : IAsymmetricKey {
        
        public DsaKey(byte[] content, AsymmetricKeyType keyType, int keySize)
        {
            Content = content;
            KeyType = keyType;
            KeySize = keySize;
            
            CipherType = CipherType.Dsa;
        }
        
        public byte[] Content { get; }
        public CipherType CipherType { get; }
        public AsymmetricKeyType KeyType { get; }
        public int KeySize { get; }
        public bool IsEncrypted => KeyType == AsymmetricKeyType.Encrypted;
        public bool IsPrivateKey => KeyType == AsymmetricKeyType.Private;
    }
}