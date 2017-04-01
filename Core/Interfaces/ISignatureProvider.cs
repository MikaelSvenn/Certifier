using Core.Model;

namespace Core.Interfaces
{
    public interface ISignatureProvider
    {
        Signature CreateSignature(IAsymmetricKey privateKey, byte[] content);
        bool VerifySignature(IAsymmetricKey publicKey, Signature signature);
    }
}