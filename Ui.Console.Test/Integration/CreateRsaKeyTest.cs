using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Provider;

namespace Ui.Console.Test.Integration
{
    [TestFixture]
    public class CreateRsaKeyTest
    {
        private Mock<FileWrapper> file;
        private Dictionary<string, string> fileOutput;

        [SetUp]
        public void SetupCreateRsaKeyTest()
        {
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
        public void Teardown()
        {
            ContainerProvider.ClearContainer();
        }

        [TestFixture]
        public class ErrorCases : CreateRsaKeyTest
        {
            [Test]
            public void ShouldNotAllowKeySizeBelow2048Bits()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "1024", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("RSA key size too small. At least 2048 bit keys are required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "--publickey", "public.pem"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.pem"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }
        }

        [TestFixture]
        public class CreateKeyPair : CreateRsaKeyTest
        {
            [Test]
            public void ShouldWritePkcs8FormattedPrivateKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.pem", "--publickey", "public.pem"});

                var content = fileOutput["private.pem"];

                Assert.IsTrue(content.Length > 1600 && content.Length < 1800);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldWritePkcs8FormattedPublicKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.pem", "--publickey", "public.pem"});

                var content = fileOutput["public.pem"];

                Assert.IsTrue(content.Length > 400 && content.Length < 500);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldCreateValidKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.pem", "--publickey", "public.pem"});

                var privateKeyContent = fileOutput["private.pem"];
                var publicKeyContent = fileOutput["public.pem"];

                var container = ContainerProvider.GetContainer();
                var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                var rsaKeyProvider = container.GetInstance<RsaKeyProvider>();

                IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);

                Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey,publicKey)));
            }
        }
    }
}