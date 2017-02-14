using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;

namespace Crypto.Generators
{
    public class RsaGenerator
    {
        private readonly SecureRandomGenerator secureRandom;

        public RsaGenerator(SecureRandomGenerator secureRandom)
        {
            this.secureRandom = secureRandom;
        }

        public AsymmetricCipherKeyPair GenerateKeyPair(int keySize)
        {
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom.Generator, keySize);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }
    }
}