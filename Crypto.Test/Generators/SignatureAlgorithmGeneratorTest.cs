using System;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class SignatureAlgorithmGeneratorTest
    {
        private IConfiguration configuration;
        private SecureRandomGenerator secureRandom;
        private SignatureAlgorithmGenerator signatureAlgorithmGenerator;

        [OneTimeSetUp]
        public void SetupSignatureAlgorithmGeneratorTest()
        {
            configuration = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 1);
            secureRandom = new SecureRandomGenerator();
            signatureAlgorithmGenerator = new SignatureAlgorithmGenerator(secureRandom);
        }

        [TestFixture]
        public class GetForSigningTest : SignatureAlgorithmGeneratorTest
        {
            [Test]
            public void ShouldThrowArgumentExceptionWhenPrivateKeyIsEncryptedAndNoPasswordIsProvided()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncryptedPrivateKey && !k.HasPassword);
                Assert.Throws<ArgumentException>(() => { signatureAlgorithmGenerator.GetForSigning(key); });
            }

            [TestFixture]
            public class ShouldReturnSha512WithMgf1For : GetForSigningTest
            {
                private IKeyProvider rsaKeyProvider;

                [OneTimeSetUp]
                public void Setup()
                {
                    var rsaKeyPairGenerator = new RsaKeyPairGenerator(secureRandom);
                    rsaKeyProvider = new RsaKeyPairProvider(configuration, rsaKeyPairGenerator, secureRandom);
                }

                [Test]
                public void Rsa()
                {
                    var key = rsaKeyProvider.CreateAsymmetricKeyPair(2048);
                    var signer = signatureAlgorithmGenerator.GetForSigning(key);

                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }

                [Test]
                public void RsaInPkcs12()
                {
                    var key = rsaKeyProvider.CreateAsymmetricPkcs12KeyPair("foobarbaz", 2048);
                    key.Password = "foobarbaz";

                    var signer = signatureAlgorithmGenerator.GetForSigning(key);

                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }
            }
        }

        [TestFixture]
        public class GetForVerifyingTest : SignatureAlgorithmGeneratorTest
        {
            [TestFixture]
            public class ShouldReturnSha512WithMgf1For : GetForVerifyingTest
            {
                private IKeyProvider rsaKeyProvider;

                [OneTimeSetUp]
                public void Setup()
                {
                    var rsaKeyPairGenerator = new RsaKeyPairGenerator(secureRandom);
                    rsaKeyProvider = new RsaKeyPairProvider(configuration, rsaKeyPairGenerator, secureRandom);
                }

                [Test]
                public void Rsa()
                {
                    var key = rsaKeyProvider.CreateAsymmetricKeyPair(2048);
                    var signer = signatureAlgorithmGenerator.GetForVerifyingSignature(key);

                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }

                [Test]
                public void RsaInPkcs12()
                {
                    var key = rsaKeyProvider.CreateAsymmetricPkcs12KeyPair("foobarbaz", 2048);
                    var signer = signatureAlgorithmGenerator.GetForVerifyingSignature(key);

                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }
            }
        }
    }
}