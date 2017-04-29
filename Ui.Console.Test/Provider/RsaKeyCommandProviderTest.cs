using Core.Model;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class RsaKeyCommandProviderTest
    {
        private RsaKeyCommandProvider provider;
        private ApplicationArguments arguments;

        [SetUp]
        public void SetupRsaKeyCommandProviderTest()
        {
            arguments = new ApplicationArguments
            {
                KeySize = 4096,
                EncryptionType = KeyEncryptionType.Pkcs,
                Password = "foopassword"

            };

            provider = new RsaKeyCommandProvider();
        }

        [TestFixture]
        public class CreateRsaKeyCommandShouldMap : RsaKeyCommandProviderTest
        {
            private CreateRsaKeyCommand command;

            [SetUp]
            public void Setup()
            {
                command = provider.GetCreateRsaKeyCommand(arguments);
            }

            [Test]
            public void KeySize()
            {
                Assert.AreEqual(4096, command.KeySize);
            }

            [Test]
            public void EncryptionType()
            {
                Assert.AreEqual(KeyEncryptionType.Pkcs, command.EncryptionType);
            }

            [Test]
            public void Password()
            {
                Assert.AreEqual("foopassword", command.Password);
            }
        }
    }
}