namespace Core.Interfaces
{
    public interface IPemFormattingProvider<T> where T : IAsymmetricKey
    {
        T GetAsDer(string pemFormatted);
        string GetAsPem(T derFormatted);
    }
}