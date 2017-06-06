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
                result = provider.GetWriteToFileCommand(signature, "signedfile.extension");
            }

            [Test]
            public void ShouldMapOutput()
            {
                Assert.AreEqual(signature, result.Out);
            }

            [Test]
            public void ShouldExtendGivenFilePath()
            {
                Assert.AreEqual("signedfile.extension", result.FilePath);
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