namespace Core.Interfaces {
    public interface IAsymmetricKeyProvider {
        IAsymmetricKey GetPublicKey(byte[] keyContent);
        IAsymmetricKey GetPrivateKey(byte[] keyContent);
        IAsymmetricKey GetEncryptedPrivateKey(byte[] keyContent);
    }
}