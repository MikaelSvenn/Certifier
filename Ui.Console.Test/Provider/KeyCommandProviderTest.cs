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
        public void SetupRsaKeyCommandProviderTest()
        {
            provider = new KeyCommandProvider();
        }

        [TestFixture]
        public class CreateKeyCommandShouldMap : KeyCommandProviderTest
        {
            private ICreateAsymmetricKeyCommand command;

            [SetUp]
            public void Setup()
            {
                command = provider.GetCreateKeyCommand(4096, KeyEncryptionType.Pkcs, "foopassword");
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