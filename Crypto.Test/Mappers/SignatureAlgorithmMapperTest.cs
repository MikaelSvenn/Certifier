using System;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Mappers
{
    [TestFixture]
    public class SignatureAlgorithmMapperTest
    {
        private IConfiguration configuration;
        private SecureRandomGenerator secureRandom;
        private SignatureAlgorithmMapper signatureAlgorithmMapper;

        [OneTimeSetUp]
        public void SetupSignatureAlgorithmMapperTest()
        {
            configuration = Mock.Of<IConfiguration>(m => m.Get<int>("SaltLengthInBytes") == 100 &&
                                                         m.Get<int>("KeyDerivationIterationCount") == 1);

            secureRandom = new SecureRandomGenerator();
            signatureAlgorithmMapper = new SignatureAlgorithmMapper(secureRandom);
        }

        [TestFixture]
        public class GetForSigningTest : SignatureAlgorithmMapperTest
        {
            [Test]
            public void ShouldThrowArgumentExceptionWhenPrivateKeyIsEncryptedAndNoPasswordIsProvided()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted);
                Assert.Throws<ArgumentException>(() =>
                {
                    signatureAlgorithmMapper.GetForSigning(key);
                });
            }

            [TestFixture]
            public class ShouldReturnSha512WithMgf1For : GetForSigningTest
            {
                private IAsymmetricKeyProvider<RsaKey> rsaKeyProvider;
                private IAsymmetricKeyPair keyPair;
                private IAsymmetricKey encryptedPrivateKey;

                [OneTimeSetUp]
                public void Setup()
                {
                    var rsaKeyPairGenerator = new RsaKeyPairGenerator(secureRandom);
                    rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
                    keyPair = rsaKeyProvider.CreateKeyPair(2048);

                    var oidToCipherTypeMapper = new OidToCipherTypeMapper();
                    var asymmetricKeyProvider = new AsymmetricKeyProvider(oidToCipherTypeMapper, rsaKeyProvider);
                    var encryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom, asymmetricKeyProvider, new PkcsEncryptionGenerator());

                    encryptedPrivateKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foobarbaz");
                }

                [Test]
                public void RsaPrivate()
                {
                    var signer = signatureAlgorithmMapper.GetForSigning(keyPair.PrivateKey);
                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }

                [Test]
                public void RsaEncrypted()
                {
                    var signer = signatureAlgorithmMapper.GetForSigning(encryptedPrivateKey, "foobarbaz");
                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }
            }
        }

        [TestFixture]
        public class GetForVerifyingTest : SignatureAlgorithmMapperTest
        {
            [TestFixture]
            public class ShouldReturnSha512WithMgf1For : GetForVerifyingTest
            {
                private IAsymmetricKeyProvider<RsaKey> rsaKeyProvider;

                [OneTimeSetUp]
                public void Setup()
                {
                    var rsaKeyPairGenerator = new RsaKeyPairGenerator(secureRandom);
                    rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
                }

                [Test]
                public void RsaPublic()
                {
                    var keyPair = rsaKeyProvider.CreateKeyPair(2048);
                    var signer = signatureAlgorithmMapper.GetForVerifyingSignature(keyPair.PublicKey);

                    Assert.AreEqual("SHA-512withRSAandMGF1", signer.AlgorithmName);
                }
            }
        }
    }
}