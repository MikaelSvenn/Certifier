namespace Core.Interfaces
{
    public interface IEcKey : IAsymmetricKey
    {
        string Curve { get; }
        bool IsCurve25519 { get; }
    }
}