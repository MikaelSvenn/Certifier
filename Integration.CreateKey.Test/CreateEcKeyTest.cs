using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console;
using Ui.Console.Provider;

namespace Integration.CreateKey.Test
{
    [TestFixture]
    public class CreateEcKeyTest
    {
        private Dictionary<string, byte[]> fileOutput;
        private Mock<FileWrapper> file;
        private EncodingWrapper encoding;

        [SetUp]
        public void SetupCreateEcKeyTest()
        {
            encoding = new EncodingWrapper();
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
        public void TeardownCreateEcKeyTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class ErrorCases : CreateEcKeyTest
        {
            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--publickey", "public"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--privatekey", "private"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }

            [Test]
            public void ShouldIndicateNonSupportedCurve()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "foo", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Curve not supported.", exception.Message);
            }
        }

        [TestFixture]
        public class CreateKeyPair : CreateEcKeyTest
        {
            [Test]
            public void ShouldWritePkcs8FormattedPrivateKey()
            {
                Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] fileContent = fileOutput["private.pem"];
                string content = encoding.GetString(fileContent);
                Assert.IsTrue(content.Length > 720 && content.Length < 780);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldWritePkcs8FormattedPublicKey()
            {
                Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] fileContent = fileOutput["public.pem"];
                string content = encoding.GetString(fileContent);
                Assert.IsTrue(content.Length > 420 && content.Length < 500);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldCreateValidPemFormattedKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] privateKeyFileContent = fileOutput["private.pem"];
                byte[] publicKeyFileContent = fileOutput["public.pem"];

                string privateKeyContent = encoding.GetString(privateKeyFileContent);
                string publicKeyContent = encoding.GetString(publicKeyFileContent);
                
                var container = ContainerProvider.GetContainer();
                var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                var ecKeyProvider = container.GetInstance<IEcKeyProvider>();

                IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);

                Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
            }

            [Test]
            public void ShouldCreateValidDerFormattedKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "der", "--privatekey", "private.der", "--publickey", "public.der"});
                
                byte[] privateKeyFileContent = fileOutput["private.der"];
                byte[] publicKeyFileContent = fileOutput["public.der"];

                var container = ContainerProvider.GetContainer();
                var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                var ecKeyProvider = container.GetInstance<IEcKeyProvider>();

                IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
            }

            [TestCase("prime239v3")]
            [TestCase("c2pnb368w1")]
            [TestCase("brainpoolP512t1")]
            [TestCase("sect571r1")]
            public void ShouldCreateValidKeyPairWithGivenCurve(string curve)
            {
                fileOutput = new Dictionary<string, byte[]>();
                Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", curve, "-t", "der", "--privatekey", "private.der", "--publickey", "public.der"});
                
                byte[] privateKeyFileContent = fileOutput["private.der"];
                byte[] publicKeyFileContent = fileOutput["public.der"];

                var container = ContainerProvider.GetContainer();
                var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                var ecKeyProvider = container.GetInstance<IEcKeyProvider>();

                IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
            }
        }
    }
}