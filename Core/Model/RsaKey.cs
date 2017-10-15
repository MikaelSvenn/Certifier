namespace Core.Model
{
    public class RsaKey : AsymmetricKey
    {
        public RsaKey(byte[] content, AsymmetricKeyType keyType, int keySize) : base(content, keyType, keySize, CipherType.Rsa) { }
    }
}