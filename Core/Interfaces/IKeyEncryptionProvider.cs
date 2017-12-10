using Core.Model;

namespace Core.Interfaces
{
    public interface IKeyEncryptionProvider
    {
        IAsymmetricKey EncryptPrivateKey(IAsymmetricKey key, string password, EncryptionType encryptionType);
        IAsymmetricKey DecryptPrivateKey(IAsymmetricKey key, string password);
    }
}