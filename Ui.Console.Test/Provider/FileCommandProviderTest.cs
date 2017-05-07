using Core.Interfaces;
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
            private WriteToTextFileCommand<IAsymmetricKey> result;

            [SetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>();
                result = provider.GetWriteKeyToTextFileCommand(key, "fooPath");
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
            private ReadFromTextFileCommand<IAsymmetricKey> result;

            [SetUp]
            public void Setup()
            {
                result = provider.GetReadKeyFromTextFileCommand("barPath");
            }

            [Test]
            public void ShouldMapFilePath()
            {
                Assert.AreEqual("barPath", result.FilePath);
            }
        }
    }
}