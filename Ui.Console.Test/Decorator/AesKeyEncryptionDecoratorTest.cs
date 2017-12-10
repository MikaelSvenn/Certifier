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
    public class AesKeyEncryptionDecoratorTest
    {
        private AesKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decorated;
        private Mock<IKeyEncryptionProvider> keyEncryptionProvider;

        [SetUp]
        public void SetupAesKeyEncryptionDecoratorTest()
        {
            decorated = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            keyEncryptionProvider = new Mock<IKeyEncryptionProvider>();
            decorator = new AesKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKey>>(decorated.Object, keyEncryptionProvider.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>();
            decorator.Execute(command);

            decorated.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldNotEncryptPrivateKeyWhenEncryptionTypeIsNotAes()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.Pkcs);
            decorator.Execute(command);

            keyEncryptionProvider.Verify(k => k.EncryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>(), EncryptionType.Aes), Times.Never());
        }

        [TestFixture]
        public class WhenEncryptionTypeIsAes : AesKeyEncryptionDecoratorTest
        {
            private WriteFileCommand<IAsymmetricKey> command;
            private IAsymmetricKey privateKey;
            private IAsymmetricKey encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                privateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                encryptedPrivateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey && k.IsEncrypted);

                keyEncryptionProvider.Setup(kep => kep.EncryptPrivateKey(privateKey, "fooPassword", EncryptionType.Aes))
                    .Returns(encryptedPrivateKey);

                command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.Aes &&
                                                                    c.Password == "fooPassword" &&
                                                                    c.Out == privateKey);
            }

            [Test]
            public void ShouldEncryptPrivateKeyWithGivenPassword()
            {
                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.EncryptPrivateKey(privateKey, "fooPassword", EncryptionType.Aes));
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