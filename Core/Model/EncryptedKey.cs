using Core.Interfaces;

namespace Core.Model
{
    public class EncryptedKey : IAsymmetricKey
    {
        public EncryptedKey(byte[] content, CipherType cipherType)
        {
            Content = content;
            CipherType = cipherType;
        }

        public byte[] Content { get; }
        public CipherType CipherType { get; }
        public AsymmetricKeyType KeyType => AsymmetricKeyType.Encrypted;
        public int KeySize => 0;
        public bool IsEncrypted => true;
        public bool IsPrivateKey => true;
    }
}