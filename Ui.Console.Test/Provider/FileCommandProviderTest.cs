using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class FileCommandProviderTest
    {
        private FileCommandProvider provider;

        [SetUp]
        public void SetupWriteToFileCommandProviderTest()
        {
            provider = new FileCommandProvider();
        }

        [TestFixture]
        public class GetReadPrivateKeyFromFileCommand : FileCommandProviderTest
        {
            private ReadKeyFromFileCommand result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadPrivateKeyFromFileCommand("barPath", "fooPassword");
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("barPath", result.FilePath);
            }

            [Test]
            public void ShouldMapPassword()
            {
                Assert.AreEqual("fooPassword", result.Password);
            }

            [Test]
            public void ShouldSetPrivateKeyTrue()
            {
                Assert.IsTrue(result.IsPrivateKey);
            }
        }

        [TestFixture]
        public class GetReadPublicKeyFromFileCommand : FileCommandProviderTest
        {
            private ReadKeyFromFileCommand result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadPublicKeyFromFileCommand("foo");
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("foo", result.FilePath);
            }
        }
        
        [TestFixture]
        public class GetReadFileCommand : FileCommandProviderTest
        {
            private ReadFileCommand<object> result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadFileCommand<object>("bazPath");
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("bazPath", result.FilePath);
            }
        }

        [TestFixture]
        public class GetWriteToFileCommand : FileCommandProviderTest
        {
            private WriteFileCommand<Signature> result;
            private Signature signature;

            [SetUp]
            public void Setup()
            {
                signature = Mock.Of<Signature>();
                result = provider.GetWriteToFileCommand(signature, "signedfile.extension", ContentType.Der, EncryptionType.Pkcs, "foopassword");
            }

            [Test]
            public void ShouldMapOutput()
            {
                Assert.AreEqual(signature, result.Out);
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("signedfile.extension", result.FilePath);
            }

            [Test]
            public void ShouldMapContentType()
            {
                Assert.AreEqual(ContentType.Der, result.ContentType);
            }

            [Test]
            public void ShouldMapEncryptionType()
            {
                Assert.AreEqual(EncryptionType.Pkcs, result.EncryptionType);
            }

            [Test]
            public void ShouldMapPassword()
            {
                Assert.AreEqual("foopassword", result.Password);
            }
        }

        [TestFixture]
        public class GetWriteKeyToFileCommand : FileCommandProviderTest
        {
            private WriteFileCommand<IAsymmetricKey> result;
            private IAsymmetricKey key;

            [SetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>();
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.Der, EncryptionType.Pkcs, "foopassword");
            }

            [Test]
            public void ShouldMapOutput()
            {
                Assert.AreEqual(key, result.Out);
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("key", result.FilePath);
            }

            [Test]
            public void ShouldMapContentType()
            {
                Assert.AreEqual(ContentType.Der, result.ContentType);
            }

            [Test]
            public void ShouldMapEncryptionType()
            {
                Assert.AreEqual(EncryptionType.Pkcs, result.EncryptionType);
            }

            [Test]
            public void ShouldMapPassword()
            {
                Assert.AreEqual("foopassword", result.Password);
            }

            [Test]
            public void ShouldMapPrivateKeyContentTypeToPemWhenContentIsSsh2()
            {
                key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.Ssh2);
                Assert.AreEqual(ContentType.Pem, result.ContentType);
            }

            [Test]
            public void ShouldMapPrivateKeyContentTypeToPemWhenContentIsOpenSshAndPrivateKeyIsNonCurve25519EcKey()
            {
                key = Mock.Of<IEcKey>(k => k.IsPrivateKey && k.CipherType == CipherType.Ec && !k.IsCurve25519);
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.OpenSsh);
                Assert.AreEqual(ContentType.Pem, result.ContentType);
            }

            [Test]
            public void ShouldMapPrivateKeyContentTypeToPemWhenContentIsOpenSshAndPrivateKeyIsNotEcKey()
            {
                key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey && k.CipherType == CipherType.Rsa);
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.OpenSsh);
                Assert.AreEqual(ContentType.Pem, result.ContentType);
            }

            [Test]
            public void ShouldNotMapPrivateKeyContentTypeToPemWhenContentIsOpenSshAndPrivateKeyIsCurve25519()
            {
                key = Mock.Of<IEcKey>(k => k.IsPrivateKey && k.CipherType == CipherType.Ec && k.IsCurve25519);
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.OpenSsh);
                Assert.AreEqual(ContentType.OpenSsh, result.ContentType);
            }
            
            [Test]
            public void ShouldMapPrivateKeyContentTypeToPemWhenContentTypeForPublicKeyIsSec1()
            {
                key = Mock.Of<IAsymmetricKey>(k => !k.IsPrivateKey);
                result = provider.GetWriteKeyToFileCommand(key, "key", ContentType.Sec1);
                Assert.AreEqual(ContentType.Pem, result.ContentType);
            }
        }
        
        [TestFixture]
        public class GetWriteToStdOutCommand : FileCommandProviderTest
        {
            private WriteToStdOutCommand<int> result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetWriteToStdOutCommand<int>(4);
            }

            [Test]
            public void ShouldMapGivenContent()
            {
                Assert.AreEqual(4, result.Out);
            }
        }
    }
}