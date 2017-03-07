using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKeyPair
    {
        IAsymmetricKey PrivateKey { get; }
        IAsymmetricKey PublicKey { get; }
        int KeyLengthInBits { get; }
        string Password { get; set; }
        bool HasPassword { get; }
    }
}