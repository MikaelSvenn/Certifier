using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace Crypto.Generators
{
    public class SecureRandomGenerator
    {
        public SecureRandomGenerator()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            Generator = new SecureRandom(randomGenerator);
        }

        public SecureRandom Generator { get; }

        public void NextBytes(byte[] result)
        {
            Generator.NextBytes(result);
        }
    }
}