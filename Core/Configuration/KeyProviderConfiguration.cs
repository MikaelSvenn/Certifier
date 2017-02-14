using System.Collections.Generic;
using Core.Interfaces;

namespace Core.Configuration
{
    public class KeyProviderConfiguration : IConfiguration
    {
        private readonly Dictionary<string, dynamic> configuration = new Dictionary<string, dynamic>
        {
            {"KeyDerivationIterationCount", 200000}
        };

        public T Get<T>(string configurationName)
        {
            return (T) configuration[configurationName];
        }
    }
}