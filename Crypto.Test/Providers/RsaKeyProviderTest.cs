using System;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class RsaKeyProviderTest
    {
        private RsaKeyPairProvider keyProvider;

        [SetUp]
        public void SetupRsaKeyProviderTest()
        {
            var config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 1);
            var secureRandomGenerator = new SecureRandomGenerator();
            var rsaGenerator = new RsaGenerator(secureRandomGenerator);

            keyProvider = new RsaKeyPairProvider(config, rsaGenerator, secureRandomGenerator);
        }

        [TestFixture]
        public class CreateAsymmetricKeyPair : RsaKeyProviderTest
        {
            private IAsymmetricKey keyPair;

            [SetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateAsymmetricKeyPair("foopassword", 1024);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey);
            }

            [Test]
            public void ShouldSetKeyLength()
            {
                Assert.AreEqual(1024, keyPair.KeyLengthInBits);
            }

            [Test]
            public void ShouldCreateValidKey()
            {
                throw new NotImplementedException("TODO: Sign with the created key and verify the created signature");
            }
        }
    }
}