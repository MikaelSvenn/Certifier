using Core.Interfaces;

namespace Core.Model
{
    public class RsaKeyPair : IAsymmetricKey
    {
        public RsaKeyPair(byte[] privateKey, byte[] publicKey, int keyLength)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
            KeyLengthInBits = keyLength;
        }

        public byte[] PrivateKey { get; }
        public byte[] PublicKey { get; }
        public int KeyLengthInBits { get; }
    }
}