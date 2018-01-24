namespace Core.Interfaces
{
    public interface ISshKeyProvider
    {
        string GetEcPublicKeyContent(IAsymmetricKey publicKey);
        string GetEd25519PublicKeyContent(IAsymmetricKey privateKey);
        string GetRsaPublicKeyContent(IAsymmetricKey publicKey);
        string GetDsaPublicKeyContent(IAsymmetricKey publicKey);
        string GetOpenSshEd25519PrivateKey(IAsymmetricKeyPair keyPair, string comment);
        bool IsSupportedCurve(string curveName);
        string GetCurveSshHeader(string curveName);
        IAsymmetricKey GetKeyFromSsh(string sshKeyContent);
    }
}