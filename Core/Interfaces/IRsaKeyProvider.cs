using Core.Model;

namespace Core.Interfaces
{
    public interface IRsaKeyProvider : IKeyProvider<RsaKey>
    {
        IAsymmetricKey GetPublicKey(byte[] exponent, byte[] modulus);
    }
}