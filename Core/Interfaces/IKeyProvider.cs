namespace Core.Interfaces
{
    public interface IKeyProvider
    {
        IAsymmetricKey CreateAsymmetricKeyPair(string password, int keySize);
    }
}