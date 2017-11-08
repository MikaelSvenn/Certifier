namespace Core.Interfaces {
    public interface ISshFormattingProvider {
        string GetAsOpenSsh(IAsymmetricKey key, string comment);
        string GetAsSsh2(IAsymmetricKey key, string comment);
        IAsymmetricKey GetAsDer(string sshKey);
        bool IsSshKey(string sshKEy);
    }
}