namespace Core.Interfaces
{
    public interface IAsymmetricKey
    {
        byte[] PrivateKey { get; }
        byte[] PublicKey { get; }
        int KeyLengthInBits { get; }
    }
}