using Core.Model;

namespace Core.Interfaces
{
    public interface IEcKeyProvider : IKeyProvider<EcKey>
    {
        IAsymmetricKeyPair CreateKeyPair(string curve);
        IEcKey GetPublicKey(byte[] q, string curve);
    }
}