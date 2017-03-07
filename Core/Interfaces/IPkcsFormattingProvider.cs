using Core.Model;

namespace Core.Interfaces
{
    public interface IPkcsFormattingProvider<T> where T : IAsymmetricKey
    {
        T GetAsDer(string pemFormatted);
        string GetAsPem(T derFormatted);
    }
}