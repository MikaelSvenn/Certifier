using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class SignatureProviderTest
    {
        private SecureRandomGenerator secureRandomGenerator;
        private SignatureProvider signatureProvider;
        private SignatureAlgorithmIdentifierMapper algorithmIdentifierMapper;
        private byte[] content;
        private Dictionary<CipherType, IAsymmetricKeyPair> keys;

        [OneTimeSetUp]
        public void SetupSignatureProviderTest()
        {
            algorithmIdentifierMapper = new SignatureAlgorithmIdentifierMapper();
            secureRandomGenerator = new SecureRandomGenerator();
            signatureProvider = new SignatureProvider(algorithmIdentifierMapper, secureRandomGenerator);

            content = secureRandomGenerator.NextBytes(2000);

            keys = new Dictionary<CipherType, IAsymmetricKeyPair>();

            var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);
            var rsaKeyProvider = new RsaKeyProvider(rsaGenerator);

            IAsymmetricKeyPair keyPair = rsaKeyProvider.CreateKeyPair(2048);

            keys.Add(keyPair.PrivateKey.CipherType, keyPair);
        }

        [TestFixture]
        public class CreateSignatureTest : SignatureProviderTest
        {
            [TestCase(CipherType.Rsa, TestName="RSA Signature")]
            public void ShouldSetSignedData(CipherType cipherType)
            {
                var keyPair = keys[cipherType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content);

                CollectionAssert.IsNotEmpty(signature.SignedData);
            }

            [TestCase(CipherType.Rsa, TestName="RSA Content")]
            public void ShouldSetSignatureContent(CipherType cipherType)
            {
                var keyPair = keys[cipherType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content);

                CollectionAssert.IsNotEmpty(signature.Content);
            }
        }

        [TestFixture]
        public class VerifySignatureTest : SignatureProviderTest
        {
            [TestCase(CipherType.Rsa,  TestName="RSA")]
            public void ShouldReturnFalseWhenSignatureIsNotValid(CipherType cipherType)
            {
                var keyPair = keys[cipherType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content);
                signature.Content[0] = (byte) (signature.Content[0] >> 1);

                Assert.IsFalse(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }

            [TestCase(CipherType.Rsa,  TestName="RSA")]
            public void ShouldReturnTrueWhenSignatureIsValid(CipherType cipherType)
            {
                var keyPair = keys[cipherType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content);

                Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }
        }
    }
}