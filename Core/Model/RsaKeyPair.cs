namespace Core.Model
{
    public class RsaKeyPair
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
        public int KeyLengthInBits => PrivateKey.Length * 8;
    }
}