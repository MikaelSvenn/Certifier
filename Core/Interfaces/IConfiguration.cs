using System.Globalization;

namespace Core.Interfaces
{
    public interface IConfiguration
    {
        T Get<T>(string configurationName);
    }
}