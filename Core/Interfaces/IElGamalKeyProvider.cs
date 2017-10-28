using Core.Model;

namespace Core.Interfaces
{
    public interface IElGamalKeyProvider : IKeyProvider<ElGamalKey>
    {
        IAsymmetricKeyPair CreateKeyPair(int keySize, bool useRfc3526Prime);
    }
}