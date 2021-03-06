using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;

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

        [SetUp]
        public void SetupSignatureProviderTest()
        {
            algorithmIdentifierMapper = new SignatureAlgorithmIdentifierMapper();
            secureRandomGenerator = new SecureRandomGenerator();
            signatureProvider = new SignatureProvider(algorithmIdentifierMapper, secureRandomGenerator, new SignerUtilitiesWrapper());

            content = secureRandomGenerator.NextBytes(2000);

            keys = new Dictionary<CipherType, IAsymmetricKeyPair>();

            var rsaGenerator = new AsymmetricKeyPairGenerator(secureRandomGenerator);
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
            public void ShouldReturnFalseWhenDataLengthExceptionIsThrown(CipherType cipherType)
            {
                var keyPair = keys[cipherType];
                var signature = signatureProvider.CreateSignature(keyPair.PrivateKey, content);

                var signer = new Mock<ISigner>();
                signer.Setup(s => s.VerifySignature(It.IsAny<byte[]>()))
                      .Throws<DataLengthException>();
                
                var signerUtilities = new Mock<SignerUtilitiesWrapper>();
                signerUtilities.Setup(w => w.GetSigner(It.IsAny<string>()))
                               .Returns(signer.Object);
                
                signatureProvider = new SignatureProvider(algorithmIdentifierMapper, secureRandomGenerator, signerUtilities.Object);
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