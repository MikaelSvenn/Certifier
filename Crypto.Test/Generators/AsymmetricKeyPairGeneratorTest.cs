using System;
using Crypto.Generators;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class AsymmetricKeyPairGeneratorTest
    {
        private AsymmetricKeyPairGenerator asymmetricKeyPairGenerator;

        [OneTimeSetUp]
        public void SetupRsaGeneratorTest()
        {
            var secureRandom = new SecureRandomGenerator();
            asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(secureRandom);
        }

        [TestFixture]
        public class GenerateRsaKeyPair : AsymmetricKeyPairGeneratorTest
        {
            private AsymmetricCipherKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = asymmetricKeyPairGenerator.GenerateRsaKeyPair(2048);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                var privateKey = (RsaKeyParameters) keyPair.Private;
                Assert.AreNotEqual(privateKey.Exponent, default(BigInteger));
                Assert.AreNotEqual(privateKey.Modulus, default(BigInteger));
                Assert.IsTrue(privateKey.IsPrivate);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                var publicKey = (RsaKeyParameters) keyPair.Public;
                Assert.AreNotEqual(publicKey.Exponent, default(BigInteger));
                Assert.AreNotEqual(publicKey.Modulus, default(BigInteger));
                Assert.IsFalse(publicKey.IsPrivate);
            }

            [Test]
            public void ShouldCreateKeysOfGivenLength()
            {
                var privateKey = (RsaKeyParameters) keyPair.Private;
                var publicKey = (RsaKeyParameters) keyPair.Public;

                Assert.AreEqual(2048, privateKey.Modulus.BitLength);
                Assert.AreEqual(2048, publicKey.Modulus.BitLength);
            }
        }

        [TestFixture]
        public class GenerateDsaKeyPair : AsymmetricKeyPairGeneratorTest
        {
            private AsymmetricCipherKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = asymmetricKeyPairGenerator.GenerateDsaKeyPair(2048);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                var privateKey = (DsaPrivateKeyParameters) keyPair.Private;
                Assert.AreNotEqual(privateKey.Parameters.G, default(BigInteger));
                Assert.AreNotEqual(privateKey.Parameters.P, default(BigInteger));
                Assert.AreNotEqual(privateKey.Parameters.Q, default(BigInteger));
                Assert.AreNotEqual(privateKey.X, default(BigInteger));
                Assert.IsTrue(privateKey.IsPrivate);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                var publicKey = (DsaKeyParameters) keyPair.Public;
                Assert.AreNotEqual(publicKey.Parameters.G, default(BigInteger));
                Assert.AreNotEqual(publicKey.Parameters.P, default(BigInteger));
                Assert.AreNotEqual(publicKey.Parameters.Q, default(BigInteger));
                Assert.IsFalse(publicKey.IsPrivate);
            }

            [Test]
            public void ShouldCreateKeysOfGivenLength()
            {
                var privateKey = (DsaKeyParameters) keyPair.Private;
                var publicKey = (DsaKeyParameters) keyPair.Public;

                Assert.AreEqual(2048, privateKey.Parameters.P.BitLength);
                Assert.AreEqual(2048, publicKey.Parameters.P.BitLength);
            }
        }

        [TestFixture]
        public class GenerateEcKeyPair : AsymmetricKeyPairGeneratorTest
        {
            private AsymmetricCipherKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = asymmetricKeyPairGenerator.GenerateEcKeyPair("curve25519");
            }
            
            [Test]
            public void ShouldCreatePrivateKey()
            {
                var privateKey = (ECPrivateKeyParameters) keyPair.Private;
                Assert.IsTrue(privateKey.IsPrivate);
                Assert.AreEqual("EC", privateKey.AlgorithmName);
                Assert.AreNotEqual(privateKey.D, default(BigInteger));
                Assert.IsNotEmpty(privateKey.Parameters.G.GetEncoded());
                Assert.AreNotEqual(privateKey.Parameters.H, default(BigInteger));
                Assert.AreNotEqual(privateKey.Parameters.N, default(BigInteger));
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                var publicKey = (ECPublicKeyParameters) keyPair.Public;
                Assert.IsFalse(publicKey.IsPrivate);
                Assert.AreEqual("EC", publicKey.AlgorithmName);
                Assert.IsNotEmpty(publicKey.Q.GetEncoded());
                Assert.IsNotEmpty(publicKey.Parameters.G.GetEncoded());
                Assert.AreNotEqual(publicKey.Parameters.H, default(BigInteger));
                Assert.AreNotEqual(publicKey.Parameters.N, default(BigInteger));
            }

            [Test]
            public void ShouldCreateKeysOfLengthTypicalToGivenCurve()
            {
                var privateKey = (ECKeyParameters) keyPair.Private;
                Assert.AreEqual(255, privateKey.Parameters.Curve.FieldSize);
                var publicKey = (ECKeyParameters) keyPair.Public;
                Assert.AreEqual(255, publicKey.Parameters.Curve.FieldSize);
            }

            [Test]
            public void ShouldCreateElliptiCurveKeyOverGivenField()
            {
                var publicKey = (ECKeyParameters) keyPair.Public;
                Assert.AreEqual("Curve25519Field", publicKey.Parameters.Curve.A.FieldName);
            }
            
            [Test]
            public void ShouldThrowExceptionWhenCurveIsNotSupported()
            {
                Assert.Throws<ArgumentException>(() => { asymmetricKeyPairGenerator.GenerateEcKeyPair("curve41417"); });
            }

            [Test]
            [Repeat(1000)]
            public void ShouldOnlyCreate32ByteCurve25519PrivateKey()
            {
                keyPair = asymmetricKeyPairGenerator.GenerateEcKeyPair("curve25519");
                var privateKey = (ECPrivateKeyParameters) keyPair.Private;
                Assert.AreEqual(32, privateKey.D.ToByteArray().Length);
            }
        }

        [TestFixture]
        public class GenerateElGamalKeyPair : AsymmetricKeyPairGeneratorTest
        {
            private AsymmetricCipherKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = asymmetricKeyPairGenerator.GenerateElGamalKeyPair(1024);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                var privateKey = (ElGamalPrivateKeyParameters) keyPair.Private;
                Assert.IsTrue(privateKey.IsPrivate);
                Assert.AreNotEqual(privateKey.X, default(BigInteger));
                Assert.AreNotEqual(privateKey.Parameters.G, default(BigInteger));
                Assert.AreNotEqual(privateKey.Parameters.P, default(BigInteger));
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                var publicKey = (ElGamalPublicKeyParameters) keyPair.Public;
                Assert.IsFalse(publicKey.IsPrivate);
                Assert.AreNotEqual(publicKey.Y, default(BigInteger));
                Assert.AreNotEqual(publicKey.Parameters.G, default(BigInteger));
                Assert.AreNotEqual(publicKey.Parameters.P, default(BigInteger));
            }

            [Test]
            public void ShouldCreateKeysOfGivenLength()
            {
                var privateKey = (ElGamalKeyParameters) keyPair.Private;
                var publicKey = (ElGamalKeyParameters) keyPair.Public;
                
                Assert.AreEqual(1024, privateKey.Parameters.P.BitLength);
                Assert.AreEqual(1024, publicKey.Parameters.P.BitLength);
            }
        }
    }
}