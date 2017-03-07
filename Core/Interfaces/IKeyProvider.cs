namespace Core.Interfaces
{
    public interface IKeyProvider
    {
        IAsymmetricKeyPair CreateAsymmetricKeyPair(int keySize);
        IAsymmetricKeyPair CreateAsymmetricPkcs12KeyPair(string password, int keySize);
    }
}