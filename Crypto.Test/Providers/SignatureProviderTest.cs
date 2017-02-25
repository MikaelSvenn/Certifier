using System.Collections.Generic;
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
        private Dictionary<AsymmetricKeyType, IAsymmetricKey> keys;

        [OneTimeSetUp]
        public void SetupSignatureProviderTest()
        {
            secureRandomGenerator = new SecureRandomGenerator();
            signatureAlgorithmGenerator = new SignatureAlgorithmGenerator(secureRandomGenerator);
            signatureProvider = new SignatureProvider(signatureAlgorithmGenerator);

            config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 10);

            content = new byte[2000];
            secureRandomGenerator.NextBytes(content);

            keys = new Dictionary<AsymmetricKeyType, IAsymmetricKey>();

            var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);
            var rsaKeyProvider = new RsaKeyPairProvider(config, rsaGenerator, secureRandomGenerator);

            IAsymmetricKey rsaPkcs12Key = rsaKeyProvider.CreateAsymmetricPkcs12KeyPair("foo", 2048);
            rsaPkcs12Key.Password = "foo";

            IAsymmetricKey rsaKey = rsaKeyProvider.CreateAsymmetricKeyPair(2048);

            keys.Add(rsaPkcs12Key.KeyType, rsaPkcs12Key);
            keys.Add(rsaKey.KeyType, rsaKey);
        }


        [TestFixture]
        public class CreateSignatureTest : SignatureProviderTest
        {
            [TestCase(AsymmetricKeyType.RsaPkcs12,  TestName="RsaPkcs12")]
            [TestCase(AsymmetricKeyType.Rsa, TestName="Rsa")]
            public void ShouldSetSignedData(AsymmetricKeyType keyType)
            {
                var key = keys[keyType];
                var signature = signatureProvider.CreateSignature(key, content);

                CollectionAssert.IsNotEmpty(signature.SignedData);
            }

            [TestCase(AsymmetricKeyType.RsaPkcs12,  TestName="RsaPkcs12")]
            [TestCase(AsymmetricKeyType.Rsa, TestName="Rsa")]
            public void ShouldSetSignatureContent(AsymmetricKeyType keyType)
            {
                var key = keys[keyType];
                var signature = signatureProvider.CreateSignature(key, content);

                CollectionAssert.IsNotEmpty(signature.Content);
            }
        }

        [TestFixture]
        public class VerifySignatureTest : SignatureProviderTest
        {
            [TestCase(AsymmetricKeyType.RsaPkcs12,  TestName="RsaPkcs12")]
            [TestCase(AsymmetricKeyType.Rsa, TestName="Rsa")]
            public void ShouldReturnFalseWhenSignatureIsNotValid(AsymmetricKeyType keyType)
            {
                var key = keys[keyType];
                var signature = signatureProvider.CreateSignature(key, content);
                signature.Content[0] = (byte) (signature.Content[0] >> 1);

                Assert.IsFalse(signatureProvider.VerifySignature(key, signature));
            }

            [TestCase(AsymmetricKeyType.RsaPkcs12,  TestName="RsaPkcs12")]
            [TestCase(AsymmetricKeyType.Rsa, TestName="Rsa")]
            public void ShouldReturnTrueWhenSignatureIsValid(AsymmetricKeyType keyType)
            {
                var key = keys[keyType];
                var signature = signatureProvider.CreateSignature(key, content);

                Assert.IsTrue(signatureProvider.VerifySignature(key, signature));
            }
        }
    }
}