namespace Core.Model 
{
    public class DsaKey : AsymmetricKey 
    {
        public DsaKey(byte[] content, AsymmetricKeyType keyType, int keySize) : base(content, keyType, keySize, CipherType.Dsa) { }
    }
}