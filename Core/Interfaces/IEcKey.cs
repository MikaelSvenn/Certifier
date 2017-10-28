namespace Core.Interfaces
{
    public interface IEcKey : IAsymmetricKey
    {
        string Curve { get; }
    }
}