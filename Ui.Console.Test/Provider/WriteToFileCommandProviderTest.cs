using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class WriteToFileCommandProviderTest
    {
        private WriteToFileCommandProvider provider;

        [SetUp]
        public void SetupWriteToFileCommandProviderTest()
        {
            provider = new WriteToFileCommandProvider();
        }

        [TestFixture]
        public class GetWriteKeyToTextFile : WriteToFileCommandProviderTest
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
                Assert.AreEqual(key, result.Content);
            }

            [Test]
            public void ShouldMapDestination()
            {
                Assert.AreEqual("fooPath", result.Destination);
            }
        }
    }
}