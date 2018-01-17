namespace Core.Interfaces
{
    public interface ISshKeyProvider
    {
        string GetEcPublicKeyContent(IAsymmetricKey key);
        string GetRsaPublicKeyContent(IAsymmetricKey key);
        string GetDsaPublicKeyContent(IAsymmetricKey key);
        string GetOpenSshEd25519PrivateKey(IAsymmetricKeyPair keyPair, string comment);
        bool IsSupportedCurve(string curveName);
        string GetCurveSshHeader(string curveName);
        IAsymmetricKey GetKeyFromSsh(string sshKeyContent);
    }
}