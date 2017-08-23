using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class KeyCommandProviderTest
    {
        private KeyCommandProvider provider;

        [SetUp]
        public void SetupKeyCommandProviderTest()
        {
            provider = new KeyCommandProvider();
        }

        [TestFixture]
        public class CreateKeyCommandShouldMap : KeyCommandProviderTest
        {
            private CreateKeyCommand<DsaKey> command;

            [SetUp]
            public void Setup()
            {
                command = provider.GetCreateKeyCommand<DsaKey>(3072);
            }

            [Test]
            public void KeySize()
            {
                Assert.AreEqual(3072, command.KeySize);
            }

            [Test]
            public void Curve()
            {
                CreateKeyCommand<EcKey> ecKeyCommand = provider.GetCreateKeyCommand<EcKey>(0, "foobar");
                Assert.AreEqual("foobar", ecKeyCommand.Curve);
            }
            
            [Test]
            public void CommandType()
            {
                Assert.IsAssignableFrom<CreateKeyCommand<DsaKey>>(command);
            }
        }

        [TestFixture]
        public class VerifyKeyCommandShouldMap : KeyCommandProviderTest
        {
            private IVerifyKeyPairCommand command;
            private IAsymmetricKey publicKey;
            private IAsymmetricKey privateKey;
            
            [SetUp]
            public void Setup()
            {
                publicKey = Mock.Of<IAsymmetricKey>();
                privateKey = Mock.Of<IAsymmetricKey>();

                command = provider.GetVerifyKeyPairCommand(publicKey, privateKey);
            }

            [Test]
            public void PublicKey()
            {
                Assert.AreEqual(publicKey, command.PublicKey);
            }

            [Test]
            public void PrivateKey()
            {
                Assert.AreEqual(privateKey, command.PrivateKey);
            }
        }
    }
}