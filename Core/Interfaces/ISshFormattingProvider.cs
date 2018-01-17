namespace Core.Interfaces {
    public interface ISshFormattingProvider {
        string GetAsOpenSshPublicKey(IAsymmetricKey key, string comment);
        string GetAsOpenSshPrivateKey(IAsymmetricKeyPair keyPair, string comment);
        string GetAsSsh2PublicKey(IAsymmetricKey key, string comment);
        IAsymmetricKey GetAsDer(string sshKey);
        bool IsSshKey(string sshKEy);
    }
}