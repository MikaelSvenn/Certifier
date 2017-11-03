namespace Core.Interfaces
{
    public interface ISshKeyProvider
    {
        string GetEcPublicKeyContent(IAsymmetricKey key);
        string GetRsaPublicKeyContent(IAsymmetricKey key);
        string GetDsaPublicKeyContent(IAsymmetricKey key);
        bool IsSupportedCurve(string curveName);
        string GetCurveSshHeader(string curveName);
    }
}