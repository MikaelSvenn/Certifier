namespace Core.Interfaces
{
    public interface IKeyProvider
    {
        IAsymmetricKey CreateAsymmetricKeyPair(int keySize);
        IAsymmetricKey CreateAsymmetricPkcs12KeyPair(string password, int keySize);
    }
}