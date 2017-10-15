using System;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

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

        public AsymmetricCipherKeyPair GenerateECKeyPair(string curve)
        {
            X9ECParameters curveParameters = ECNamedCurveTable.GetByName(curve) ?? CustomNamedCurves.GetByName(curve);
            if (curveParameters == null)
            {
                throw new ArgumentException("Curve not supported.");
            }
            
            var ecDomainParameters = new ECDomainParameters(curveParameters.Curve,
                                                            curveParameters.G,
                                                            curveParameters.N,
                                                            curveParameters.H,
                                                            curveParameters.GetSeed());
            
            var keyGenerationParamters = new ECKeyGenerationParameters(ecDomainParameters, secureRandom.Generator);
            var keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParamters);
            
            return keyPairGenerator.GenerateKeyPair();
        }

        public AsymmetricCipherKeyPair GenerateElGamalKeyPair(int keySize, BigInteger prime = null, BigInteger generator = null)
        {
            ElGamalParameters elGamalParameters;
            if (prime != null && generator != null)
            {
                elGamalParameters = new ElGamalParameters(prime, generator);
            }
            else
            {
                var elGamalParameterGenerator = new ElGamalParametersGenerator();
                elGamalParameterGenerator.Init(keySize, 64, secureRandom.Generator);

                elGamalParameters = elGamalParameterGenerator.GenerateParameters();
            }
            
            var keyGenerationParameters = new ElGamalKeyGenerationParameters(secureRandom.Generator, elGamalParameters);
            var keyPairGenerator = new ElGamalKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }
    }
}