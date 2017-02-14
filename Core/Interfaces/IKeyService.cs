namespace Core.Interfaces
{
    public interface IKeyService
    {
        IAsymmetricKey CreateAsymmetricKeyPair(string password, int keySizeInBits = 4096);
    }
}