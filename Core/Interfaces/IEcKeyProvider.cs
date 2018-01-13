using Core.Model;

namespace Core.Interfaces
{
    public interface IEcKeyProvider : IKeyProvider<EcKey>
    {
        IAsymmetricKeyPair CreateKeyPair(string curve);
        IEcKey GetPublicKey(byte[] q, string curve);
        IEcKey GetPkcs8PrivateKeyAsSec1(IEcKey key);
        IEcKey GetSec1PrivateKeyAsPkcs8(byte[] sec1KeyContent);
        byte[] GetEd25519PublicKeyFromCurve25519(byte[] q);
    }
}