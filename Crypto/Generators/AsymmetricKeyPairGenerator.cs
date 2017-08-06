using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace Crypto.Generators
{
    public class AsymmetricKeyPairGenerator
    {
        private readonly SecureRandomGenerator secureRandom;

        public AsymmetricKeyPairGenerator(SecureRandomGenerator secureRandom)
        {
            this.secureRandom = secureRandom;
        }

        public AsymmetricCipherKeyPair GenerateRsaKeyPair(int keySize)
        {
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom.Generator, keySize);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }

        public AsymmetricCipherKeyPair GenerateDsaKeyPair(int keySize)
        {
            var dsaParameterGenerator = new DsaParametersGenerator(new Sha256Digest());
            
            //Key size is fixed to be either 2048 or 3072 (Table C.1 on FIPS 186-3)
            dsaParameterGenerator.Init(new DsaParameterGenerationParameters(keySize, 256, 128, secureRandom.Generator));

            DsaParameters dsaParameters = dsaParameterGenerator.GenerateParameters();
            var keyGenerationParameters = new DsaKeyGenerationParameters(secureRandom.Generator, dsaParameters);
            
            var keyPairGenerator = new DsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }
    }
}