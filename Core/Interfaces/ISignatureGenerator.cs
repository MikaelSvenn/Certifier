using Core.Model;

namespace Core.Interfaces
{
    public interface ISignatureGenerator
    {
        Signature Sign();
        bool VerifySignature();
    }
}