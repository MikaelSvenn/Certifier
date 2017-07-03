using Core.Model;

namespace Core.Interfaces
{
    public interface IKeyProvider<out T> where T : IAsymmetricKey
    {
        IAsymmetricKeyPair CreateKeyPair(int keySize);
        T GetKey(byte[] content, AsymmetricKeyType keyType);
        bool VerifyKeyPair(IAsymmetricKeyPair keyPair);
    }
}