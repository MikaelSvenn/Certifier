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

        public virtual byte[] NextBytes(int length)
        {
            var result = new byte[length];
            Generator.NextBytes(result);

            return result;
        }
    }
}