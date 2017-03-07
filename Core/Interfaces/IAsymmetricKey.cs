using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKey
    {
        byte[] Content { get; }
        AsymmetricKeyType KeyType { get; }
        int KeySize { get; }
        bool IsEncrypted { get; }
    }
}