using Crypto.Generators;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class RsaKeyPairGeneratorTest
    {
        private RsaKeyPairGenerator rsaKeyPairGenerator;

        [OneTimeSetUp]
        public void SetupRsaGeneratorTest()
        {
            var secureRandom = new SecureRandomGenerator();
            rsaKeyPairGenerator = new RsaKeyPairGenerator(secureRandom);
        }

        [TestFixture]
        public class GenerateKeyPair : RsaKeyPairGeneratorTest
        {
            private AsymmetricCipherKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = rsaKeyPairGenerator.GenerateKeyPair(1024);
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

                Assert.AreEqual(1024, privateKey.Modulus.BitLength);
                Assert.AreEqual(1024, publicKey.Modulus.BitLength);
            }
        }
    }
}