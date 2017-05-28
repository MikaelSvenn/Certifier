using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;

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
        public class GetWriteKeyToTextFile : FileCommandProviderTest
        {
            private IAsymmetricKey key;
            private WriteToFileCommand<IAsymmetricKey> result;

            [SetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>();
                result = provider.GetWriteKeyToFileCommand(key, "fooPath");
            }

            [Test]
            public void ShouldMapContent()
            {
                Assert.AreEqual(key, result.Result);
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("fooPath", result.FilePath);
            }
        }

        [TestFixture]
        public class GetReadKeyFromTextFile : FileCommandProviderTest
        {
            private ReadKeyFromFileCommand result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadKeyFromFileCommand("barPath", "fooPassword");
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
        }

        [TestFixture]
        public class GetReadFromFileCommand : FileCommandProviderTest
        {
            private ReadFromFileCommand result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadFromFileCommand("bazPath");
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("bazPath", result.FilePath);
            }
        }

        [TestFixture]
        public class GetWriteSignatureToTextFileCommand : FileCommandProviderTest
        {
            private WriteToFileCommand<Signature> result;
            private Signature signature;

            [SetUp]
            public void Setup()
            {
                signature = Mock.Of<Signature>();
                result = provider.GetWriteSignatureToFileCommand(signature, "signedfile.extension");
            }

            [Test]
            public void ShouldMapResult()
            {
                Assert.AreEqual(signature, result.Result);
            }

            [Test]
            public void ShouldExtendGivenFilePathWithSignatureSuffix()
            {
                Assert.AreEqual("signedfile.extension.signature", result.FilePath);
            }
        }
    }
}