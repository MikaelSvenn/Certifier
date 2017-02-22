namespace Core.Interfaces
{
    public interface ISignatureService
    {
        byte[] CreateSignature(IAsymmetricKey key, byte[] content);
        bool IsValidSignature(IAsymmetricKey key, byte[] signature);
    }
}