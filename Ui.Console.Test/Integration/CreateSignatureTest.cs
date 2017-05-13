using System;
using System.Collections.Generic;
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
        private Dictionary<string, string> fileOutput;

        [SetUp]
        public void SetupCreateSignatureTest()
        {
            var random = new SecureRandomGenerator();
            fileContent = random.NextBytes(10000);

            file = new Mock<FileWrapper>();
            fileOutput = new Dictionary<string, string>();

            file = new Mock<FileWrapper>();
            file.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, content) =>
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

        [TestFixture]
        public class CreateSignatureWithRsaKey : CreateSignatureTest
        {
            private IAsymmetricKeyPair keyPair;

            [SetUp]
            public void Setup()
            {
                var rsaKeyPairGenerator = new RsaKeyPairGenerator(new SecureRandomGenerator());
                var rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
                var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), rsaKeyProvider);
                var pkcs8Formatter = new Pkcs8FormattingProvider(asymmetricKeyProvider);

                keyPair = rsaKeyProvider.CreateKeyPair(2048);
                var rsaKey = keyPair.PrivateKey;
                privateKey = pkcs8Formatter.GetAsPem(rsaKey);

                file.Setup(f => f.ReadAllText("private.pem"))
                    .Returns(privateKey);

                file.Setup(f => f.ReadAllBytes("foo.file"))
                    .Returns(fileContent);

                Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-f", "foo.file"});
            }

            [Test]
            public void ShouldWriteBase64EncodedSignatureToFile()
            {
                string signature = fileOutput["foo.file.signature"];

                Assert.IsNotEmpty(signature);
                Assert.DoesNotThrow(() => Convert.FromBase64String(signature));
            }

            [Test]
            public void ShouldCreateValidSignature()
            {
                Container container = ContainerProvider.GetContainer();
                var signatureProvider = container.GetInstance<SignatureProvider>();

                string encodedSignature = fileOutput["foo.file.signature"];
                var signature = new Signature
                {
                    Content = Convert.FromBase64String(encodedSignature),
                    SignedData = fileContent
                };

                Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }
        }
    }
}