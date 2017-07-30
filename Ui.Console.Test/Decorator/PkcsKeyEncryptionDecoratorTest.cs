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
        private PkcsKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decorated;
        private Mock<IKeyEncryptionProvider> keyEncryptionProvider;

        [SetUp]
        public void SetupPkcsKeyEncryptionDecoratorTest()
        {
            decorated = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            keyEncryptionProvider = new Mock<IKeyEncryptionProvider>();
            decorator = new PkcsKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKey>>(decorated.Object, keyEncryptionProvider.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>();
            decorator.Execute(command);

            decorated.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldNotEncryptPrivateKeyWhenEncryptionTypeIsNotPkcs()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.None);
            decorator.Execute(command);

            keyEncryptionProvider.Verify(k => k.EncryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()), Times.Never());
        }

        [TestFixture]
        public class WhenEncryptionTypeIsPkcs : PkcsKeyEncryptionDecoratorTest
        {
            private WriteFileCommand<IAsymmetricKey> command;
            private IAsymmetricKey privateKey;
            private IAsymmetricKey encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                privateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                encryptedPrivateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey && k.IsEncrypted);

                keyEncryptionProvider.Setup(kep => kep.EncryptPrivateKey(privateKey, "fooPassword"))
                    .Returns(encryptedPrivateKey);

                command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.Pkcs &&
                                                                    c.Password == "fooPassword" &&
                                                                    c.Out == privateKey);
            }

            [Test]
            public void ShouldEncryptPrivateKeyWithGivenPassword()
            {
                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.EncryptPrivateKey(privateKey, "fooPassword"));
            }

            [Test]
            public void ShouldReplaceKeyWithEncryptedKey()
            {
                decorator.Execute(command);
                Assert.AreEqual(encryptedPrivateKey, command.Out);
            }
        }

    }
}