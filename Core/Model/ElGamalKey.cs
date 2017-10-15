namespace Core.Model
{
    public class ElGamalKey : AsymmetricKey
    {
        public ElGamalKey(byte[] content, AsymmetricKeyType keyType, int keyLength) : base(content, keyType, keyLength, CipherType.ElGamal) { }
    }
}