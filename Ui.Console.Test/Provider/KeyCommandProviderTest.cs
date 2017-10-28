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
                command = provider.GetCreateKeyCommand<DsaKey>(3072, true);
            }

            [Test]
            public void KeySize()
            {
                Assert.AreEqual(3072, command.KeySize);
            }

            [Test]
            public void UseRfc3526Prime()
            {
                Assert.AreEqual(true, command.UseRfc3526Prime);
            }

            [Test]
            public void Curve()
            {
                CreateKeyCommand<IEcKey> createEcKeyCommand = provider.GetCreateKeyCommand<EcKey>("foo");
                Assert.AreEqual("foo", createEcKeyCommand.Curve);
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