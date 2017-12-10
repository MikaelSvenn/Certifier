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
    public class AesKeyDecryptionDecoratorTest
    {
        private AesKeyDecryptionDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedCommandHandler;
        private Mock<IKeyEncryptionProvider> keyEncryptionProvider;
        private ReadKeyFromFileCommand command;

        [SetUp]
        public void SetupPkcsKeyDecryptionDecoratorTest()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            keyEncryptionProvider = new Mock<IKeyEncryptionProvider>();
            decorator = new AesKeyDecryptionDecorator<ReadKeyFromFileCommand>(decoratedCommandHandler.Object, keyEncryptionProvider.Object);
            command = new ReadKeyFromFileCommand();
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var key = Mock.Of<IAsymmetricKey>();
            command.Result = key;

            decorator.Execute(command);
            decoratedCommandHandler.Verify(dch => dch.Execute(command));
        }

        [TestFixture]
        public class WhenKeyIsAesEncrypted : AesKeyDecryptionDecoratorTest
        {
            private IAsymmetricKey expectedKey;

            [SetUp]
            public void Setup()
            {
                expectedKey = Mock.Of<IAsymmetricKey>();

                keyEncryptionProvider
                    .Setup(kep => kep.DecryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()))
                    .Returns(expectedKey);
            }

            public void ShouldDecryptPrivateKey()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.AesEncrypted);
                command.Result = key;
                command.Password = "foobar";

                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.DecryptPrivateKey(key, "foobar"));
            }

            public void ShouldSetDecryptedKeyAsResult()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.AesEncrypted);
                command.Result = key;

                decorator.Execute(command);
                Assert.AreEqual(expectedKey, command.Result);
            }

            public void ShouldSetOriginalEncryptionType()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.AesEncrypted);
                command.Result = key;

                decorator.Execute(command);
                Assert.AreEqual(EncryptionType.Aes, command.OriginalEncryptionType);
            }
        }

        [TestFixture]
        public class ShouldNotDecryptPrivateKey : AesKeyDecryptionDecoratorTest
        {
            [Test]
            public void WhenKeyIsNotEncrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => !k.IsEncrypted && k.CipherType == CipherType.AesEncrypted);
                command.Result = key;

                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.DecryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void WhenKeyIsNotAesEncrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.Pkcs12Encrypted);
                command.Result = key;

                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.DecryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldNotSetOriginalEncryptionTypeWhenKeyIsNotDecrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => !k.IsEncrypted);
                command.Result = key;

                decorator.Execute(command);
                Assert.AreEqual(EncryptionType.None, command.OriginalEncryptionType);
            }
        }
    }
}