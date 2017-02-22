using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;

namespace Crypto.Generators
{
    public class RsaKeyPairGenerator
    {
        private readonly SecureRandomGenerator secureRandom;

        public RsaKeyPairGenerator(SecureRandomGenerator secureRandom)
        {
            this.secureRandom = secureRandom;
        }

        public AsymmetricCipherKeyPair GenerateKeyPair(int keySize)
        {
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom.Generator, keySize);

            var keyPairGenerator = new Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }
    }
}