using System;
using System.Collections.Generic;
using System.Text;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Provider;

namespace Ui.Console.Test.Integration
{
    [TestFixture]
    public class CreateSignatureTest
    {
        private string privateKey;
        private byte[] fileContent;
        private Mock<FileWrapper> file;
        private Dictionary<string, byte[]> fileOutput;

        [SetUp]
        public void SetupCreateSignatureTest()
        {
            var random = new SecureRandomGenerator();
            fileContent = random.NextBytes(10000);

            file = new Mock<FileWrapper>();
            fileOutput = new Dictionary<string, byte[]>();

            file = new Mock<FileWrapper>();
            file.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((path, content) =>
                {
                    fileOutput.Add(path, content);
                });

            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
        }

        [TearDown]
        public void TeardownCreateSignatureTest()
        {
            ContainerProvider.ClearContainer();
        }

        protected IAsymmetricKeyPair SetupWithRsaKey()
        {
            var rsaKeyPairGenerator = new RsaKeyPairGenerator(new SecureRandomGenerator());
            var rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
            var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), rsaKeyProvider);
            var pkcs8Formatter = new Pkcs8FormattingProvider(asymmetricKeyProvider);

            var keyPair = rsaKeyProvider.CreateKeyPair(2048);
            var rsaKey = keyPair.PrivateKey;
            privateKey = pkcs8Formatter.GetAsPem(rsaKey);

            file.Setup(f => f.ReadAllBytes("private.pem"))
                .Returns(Encoding.UTF8.GetBytes(privateKey));

            file.Setup(f => f.ReadAllBytes("foo.file"))
                .Returns(fileContent);

            return keyPair;
        }

        [TestFixture]
        public class CreateWithRsaSignature : CreateSignatureTest
        {
            private IAsymmetricKeyPair keyPair;

            [SetUp]
            public void SetupCreateRsaSignature()
            {
                keyPair = SetupWithRsaKey();
            }

            [Test]
            public void ShouldWriteBase64EncodedSignatureToFile()
            {
                Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", "foo.file"});

                byte[] signatureContent = fileOutput["foo.file.signature"];
                var signature = Encoding.UTF8.GetString(signatureContent);

                Assert.IsNotEmpty(signature);
                Assert.DoesNotThrow(() => Convert.FromBase64String(signature));
            }

            [Test]
            public void ShouldCreateValidSignature()
            {
                Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", "foo.file"});

                Container container = ContainerProvider.GetContainer();
                var signatureProvider = container.GetInstance<SignatureProvider>();

                byte[] encodedSignatureFile = fileOutput["foo.file.signature"];
                var encodedSignature = Encoding.UTF8.GetString(encodedSignatureFile);
                
                var signature = new Signature
                {
                    Content = Convert.FromBase64String(encodedSignature),
                    SignedData = fileContent
                };

                Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }
        }

        [TestFixture]
        public class ErrorCases : CreateSignatureTest
        {
            [SetUp]
            public void SetupErrorCases()
            {
                SetupWithRsaKey();
            }

            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() =>
                {
                    Certifier.Main(new []{"-c", "signature", "-i", "foo.file"});
                });

                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingTargetFile()
            {
                var exception = Assert.Throws<ArgumentException>(() =>
                {
                    Certifier.Main(new []{"-c", "signature", "--privatekey", "private.pem"});
                });

                Assert.AreEqual("Target file is required.", exception.Message);
            }
        }
    }
}