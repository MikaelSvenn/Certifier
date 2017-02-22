using Core.Model;

namespace Core.Interfaces
{
    public interface ISignatureProvider
    {
        Signature CreateSignature(IAsymmetricKey asymmetricKey, byte[] content);
        bool VerifySignature(IAsymmetricKey asymmetricKey, Signature signature);
    }
}