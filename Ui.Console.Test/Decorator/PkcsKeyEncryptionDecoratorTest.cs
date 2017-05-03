using System.Net;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class PkcsKeyEncryptionDecoratorTest
    {
        private PkcsKeyEncryptionDecorator<ICreateAsymmetricKeyCommand> decorator;
        private Mock<ICommandHandler<ICreateAsymmetricKeyCommand>> decorated;
        private Mock<IKeyEncryptionProvider> keyEncryptionProvider;

        [SetUp]
        public void SetupPkcsKeyEncryptionDecoratorTest()
        {
            decorated = new Mock<ICommandHandler<ICreateAsymmetricKeyCommand>>();
            keyEncryptionProvider = new Mock<IKeyEncryptionProvider>();
            decorator = new PkcsKeyEncryptionDecorator<ICreateAsymmetricKeyCommand>(decorated.Object, keyEncryptionProvider.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>();
            decorator.Execute(command);

            decorated.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldNotEncryptPrivateKeyWhenEncryptionTypeIsNotPkcs()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.EncryptionType == KeyEncryptionType.None);
            decorator.Execute(command);

            keyEncryptionProvider.Verify(k => k.EncryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()), Times.Never());
        }

        [TestFixture]
        public class WhenEncryptionTypeIsPkcs : PkcsKeyEncryptionDecoratorTest
        {
            private ICreateAsymmetricKeyCommand command;
            private IAsymmetricKey privateKey;
            private IAsymmetricKey encryptedPrivateKey;
            private IAsymmetricKey publicKey;

            [SetUp]
            public void Setup()
            {
                privateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                encryptedPrivateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey && k.IsEncrypted);
                publicKey = Mock.Of<IAsymmetricKey>(k => !k.IsPrivateKey);

                keyEncryptionProvider.Setup(kep => kep.EncryptPrivateKey(privateKey, "fooPassword"))
                    .Returns(encryptedPrivateKey);

                var keyPair = Mock.Of<IAsymmetricKeyPair>(kp => kp.PrivateKey == privateKey && kp.PublicKey == publicKey);
                command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.EncryptionType == KeyEncryptionType.Pkcs &&
                                                                    c.Password == "fooPassword" &&
                                                                    c.Result == keyPair);
            }

            [Test]
            public void ShouldEncryptPrivateKeyWithGivenPassword()
            {
                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.EncryptPrivateKey(privateKey, "fooPassword"));
            }

            [Test]
            public void ShouldReplacePrivateKeyWithEncryptedPrivateKey()
            {
                decorator.Execute(command);
                Assert.AreEqual(encryptedPrivateKey, command.Result.PrivateKey);
            }

            [Test]
            public void ShouldNotReplacePublicKey()
            {
                decorator.Execute(command);
                Assert.AreEqual(publicKey, command.Result.PublicKey);
            }
        }

    }
}