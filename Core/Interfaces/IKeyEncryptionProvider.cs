namespace Core.Interfaces
{
    public interface IKeyEncryptionProvider
    {
        IAsymmetricKey EncryptPrivateKey(IAsymmetricKey key, string password);
        IAsymmetricKey DecryptPrivateKey(IAsymmetricKey key, string password);
    }
}