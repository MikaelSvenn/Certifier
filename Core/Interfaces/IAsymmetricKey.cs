using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKey
    {
        byte[] PrivateKey { get; }
        byte[] PublicKey { get; }
        int KeyLengthInBits { get; }
        AsymmetricKeyType KeyType { get; }
        bool IsEncryptedPrivateKey { get; }
        string Password { get; set; }
        bool HasPassword { get; }
    }
}