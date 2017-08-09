namespace Core.Interfaces
{
    public interface IAsymmetricKeyPair
    {
        IAsymmetricKey PrivateKey { get; }
        IAsymmetricKey PublicKey { get; }
        string Password { get; set; }
        bool HasPassword { get; }
    }
}