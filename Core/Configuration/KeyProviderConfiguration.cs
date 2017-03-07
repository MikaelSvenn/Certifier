using System.Collections.Generic;
using Core.Interfaces;

namespace Core.Configuration
{
    public class KeyProviderConfiguration : IConfiguration
    {
        private readonly Dictionary<string, dynamic> configuration = new Dictionary<string, dynamic>
        {
            {"SaltLengthInBytes", 1024},
            {"KeyDerivationIterationCount", 200000}
        };

        public T Get<T>(string configurationName)
        {
            return (T) configuration[configurationName];
        }
    }
}