using Core.Interfaces;

namespace Core.Services
{
    public class SignatureService : ISignatureService
    {
        public byte[] CreateSignature(IAsymmetricKey key, byte[] content)
        {
            throw new System.NotImplementedException();
        }

        public bool IsValidSignature(IAsymmetricKey key, byte[] content)
        {
            throw new System.NotImplementedException();
        }
    }
}