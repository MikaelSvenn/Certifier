using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKey
    {
        byte[] Content { get; }
        CipherType CipherType { get; }
        AsymmetricKeyType KeyType { get; }
        int KeySize { get; }
        bool IsEncrypted { get; }
        bool IsPrivateKey { get; }
    }
}