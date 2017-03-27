using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
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
        private SignatureAlgorithmMapper signatureAlgorithmMapper;
        private byte[] content;
        private Dictionary<AsymmetricKeyType, IAsymmetricKeyPair> keys;

        [OneTimeSetUp]
        public void SetupSignatureProviderTest()
        {
            secureRandomGenerator = new SecureRandomGenerator();
            signatureAlgorithmMapper = new SignatureAlgorithmMapper(secureRandomGenerator);
            signatureProvider = new SignatureProvider(signatureAlgorithmMapper);

            config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 10 &&
                m.Get<int>("SaltLengthInBytes") == 100);

            content = secureRandomGenerator.NextBytes(2000);

            keys = new Dictionary<AsymmetricKeyType, IAsymmetricKeyPair>();

            var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);
            var rsaKeyProvider = new RsaKeyProvider(config, rsaGenerator, secureRandomGenerator);

            IAsymmetricKeyPair rsaPkcs12Key = rsaKeyProvider.CreatePkcs12KeyPair("foo", 2048);
            rsaPkcs12Key.Password = "foo";

            IAsymmetricKeyPair rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);

            keys.Add(rsaPkcs12Key.PrivateKey.KeyType, rsaPkcs12Key);
            keys.Add(rsaKeyPair.PrivateKey.KeyType, rsaKeyPair);
        }

        [TestFixture]
        public class CreateSignatureTest : SignatureProviderTest
        {
            [TestCase(AsymmetricKeyType.Encrypted,  TestName="Encrypted")]
            [TestCase(AsymmetricKeyType.Private, TestName="Private")]
            public void ShouldSetSignedData(AsymmetricKeyType keyType)
            {
                var keyPair = keys[keyType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content, keyPair.Password);

                CollectionAssert.IsNotEmpty(signature.SignedData);
            }

            [TestCase(AsymmetricKeyType.Encrypted,  TestName="Encrypted")]
            [TestCase(AsymmetricKeyType.Private, TestName="Private")]
            public void ShouldSetSignatureContent(AsymmetricKeyType keyType)
            {
                var keyPair = keys[keyType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content, keyPair.Password);

                CollectionAssert.IsNotEmpty(signature.Content);
            }
        }

        [TestFixture]
        public class VerifySignatureTest : SignatureProviderTest
        {
            [TestCase(AsymmetricKeyType.Encrypted,  TestName="Encrypted")]
            [TestCase(AsymmetricKeyType.Private, TestName="Private")]
            public void ShouldReturnFalseWhenSignatureIsNotValid(AsymmetricKeyType keyType)
            {
                var keyPair = keys[keyType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content, keyPair.Password);
                signature.Content[0] = (byte) (signature.Content[0] >> 1);

                Assert.IsFalse(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }

            [TestCase(AsymmetricKeyType.Encrypted,  TestName="Encrypted")]
            [TestCase(AsymmetricKeyType.Private, TestName="Private")]
            public void ShouldReturnTrueWhenSignatureIsValid(AsymmetricKeyType keyType)
            {
                var keyPair = keys[keyType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content, keyPair.Password);

                Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }
        }
    }
}