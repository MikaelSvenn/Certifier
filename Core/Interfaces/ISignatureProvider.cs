using Core.Model;

namespace Core.Interfaces
{
    public interface ISignatureProvider
    {
        Signature CreateSignature(IAsymmetricKeyPair asymmetricKeyPair, byte[] content);
        bool VerifySignature(IAsymmetricKeyPair asymmetricKeyPair, Signature signature);
    }
}