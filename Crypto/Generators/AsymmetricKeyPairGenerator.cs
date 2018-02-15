using System;
using Org.BouncyCastle.Asn1;
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

        // Based on RFCs 5480 and 5915, a named curve is used whenever possible.
        // Curve25519 is supported by BC, but not yet standardized and thus does not have an oid. 
        public AsymmetricCipherKeyPair GenerateEcKeyPair(string curve)
        {
            if (curve.Equals("curve25519"))
            {
                return GenerateCurve25519();
            }

            DerObjectIdentifier curveOid = ECNamedCurveTable.GetOid(curve) ?? CustomNamedCurves.GetOid(curve);
            if (curveOid == null)
            {
                throw new ArgumentException($"Curve {curve} is not supported.");
            }

            var keyGenerationParameters = new ECKeyGenerationParameters(curveOid, secureRandom.Generator);
            var keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return keyPairGenerator.GenerateKeyPair();
        }

        private AsymmetricCipherKeyPair GenerateCurve25519()
        {
            var keyGenerationParameters = Curve25519KeyGenerationParameters;

            var keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            return CreateEcKeyPairAndEnsure32BytePrivateKey(keyPairGenerator);
        }

        private ECKeyGenerationParameters Curve25519KeyGenerationParameters
        {
            get
            {
                X9ECParameters curveParameters = CustomNamedCurves.GetByName("curve25519");
                var ecDomainParameters = new ECDomainParameters(curveParameters.Curve,
                                                                curveParameters.G,
                                                                curveParameters.N,
                                                                curveParameters.H,
                                                                curveParameters.GetSeed());

                ECKeyGenerationParameters keyGenerationParameters = new ECKeyGenerationParameters(ecDomainParameters, secureRandom.Generator);
                return keyGenerationParameters;
            }
        }

        private static AsymmetricCipherKeyPair CreateEcKeyPairAndEnsure32BytePrivateKey(ECKeyPairGenerator keyPairGenerator)
        {
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();
            while (((ECPrivateKeyParameters) keyPair.Private).D.ToByteArray().Length != 32)
            {
                keyPair = keyPairGenerator.GenerateKeyPair();
            }

            return keyPair;
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