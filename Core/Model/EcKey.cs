namespace Core.Model
{
    public class EcKey : AsymmetricKey 
    {
        public EcKey(byte[] content, AsymmetricKeyType keyType, int keyLength) : base(content, keyType, keyLength, CipherType.Ec) { }
    }
}