using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class SignatureProviderTest
    {
        private IConfiguration config;
        private SecureRandomGenerator secureRandomGenerator;
        private SignatureProvider signatureProvider;
        private SignatureAlgorithmGenerator signatureAlgorithmGenerator;
        private byte[] content;

        [SetUp]
        public void SetupSignatureProviderTest()
        {
            secureRandomGenerator = new SecureRandomGenerator();
            signatureAlgorithmGenerator = new SignatureAlgorithmGenerator(secureRandomGenerator);
            signatureProvider = new SignatureProvider(signatureAlgorithmGenerator);

            config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 10);

            content = new byte[2000];
            secureRandomGenerator.NextBytes(content);
        }

        [TestFixture]
        public class RsaPkcs12SignatureTest : SignatureProviderTest
        {
            private IAsymmetricKey key;

            [SetUp]
            public void SetupRsaPkcs12SignatureTest()
            {
                var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);
                var keyProvider = new RsaKeyPairProvider(config, rsaGenerator, secureRandomGenerator);

                key = keyProvider.CreateAsymmetricPkcs12KeyPair("foo", 2048);
                key.Password = "foo";
            }

            [TestFixture]
            public class CreateSignatureTest : RsaPkcs12SignatureTest
            {
                private Signature signature;

                [SetUp]
                public void Setup()
                {
                    signature = signatureProvider.CreateSignature(key, content);
                }

                [Test]
                public void ShouldReturnSignatureWithSignedData()
                {
                    CollectionAssert.IsNotEmpty(signature.SignedData);
                }

                [Test]
                public void ShouldReturnSignatureWithSignatureContent()
                {
                    CollectionAssert.IsNotEmpty(signature.Content);
                }
            }

            [TestFixture]
            public class VerifySignatureTest : RsaPkcs12SignatureTest
            {
                private Signature signature;

                [SetUp]
                public void Setup()
                {
                    signature = signatureProvider.CreateSignature(key, content);
                }

                [Test]
                public void ShouldReturnFalseWhenSignatureIsNotValid()
                {
                    signature.Content[0] = (byte) (signature.Content[0] >> 1);
                    Assert.IsFalse(signatureProvider.VerifySignature(key, signature));
                }

                [Test]
                public void ShouldReturnTrueWhenSignatureIsValid()
                {
                    Assert.True(signatureProvider.VerifySignature(key, signature));
                }
            }
        }
    }
}