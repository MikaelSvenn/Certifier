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
    public class PkcsKeyDecryptionDecoratorTest
    {
        private PkcsKeyDecryptionDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedCommandHandler;
        private Mock<IKeyEncryptionProvider> keyEncryptionProvider;
        private ReadKeyFromFileCommand command;

        [SetUp]
        public void SetupPkcsKeyDecryptionDecoratorTest()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            keyEncryptionProvider = new Mock<IKeyEncryptionProvider>();
            decorator = new PkcsKeyDecryptionDecorator<ReadKeyFromFileCommand>(decoratedCommandHandler.Object, keyEncryptionProvider.Object);
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
        public class WhenKeyIsPkcsEncrypted : PkcsKeyDecryptionDecoratorTest
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

            [TestCase(CipherType.Pkcs5Encrypted, TestName = "PKCS5")]
            [TestCase(CipherType.Pkcs12Encrypted, TestName = "PKCS12")]
            public void ShouldDecryptPrivateKey(CipherType cipherType)
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == cipherType);
                command.Result = key;
                command.Password = "foobar";

                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.DecryptPrivateKey(key, "foobar"));
            }

            [TestCase(CipherType.Pkcs5Encrypted, TestName = "PKCS5")]
            [TestCase(CipherType.Pkcs12Encrypted, TestName = "PKCS12")]
            public void ShouldSetDecryptedKeyAsResult(CipherType cipherType)
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == cipherType);
                command.Result = key;

                decorator.Execute(command);
                Assert.AreEqual(expectedKey, command.Result);
            }

            public void ShouldSetOriginalEncryptionType()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.Pkcs12Encrypted);
                command.Result = key;

                decorator.Execute(command);
                Assert.AreEqual(EncryptionType.Pkcs, command.OriginalEncryptionType);
            }
        }

        [TestFixture]
        public class ShouldNotDecryptPrivateKey : PkcsKeyDecryptionDecoratorTest
        {
            [Test]
            public void WhenKeyIsNotEncrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => !k.IsEncrypted && k.CipherType == CipherType.Pkcs12Encrypted);
                command.Result = key;

                decorator.Execute(command);
                keyEncryptionProvider.Verify(kep => kep.DecryptPrivateKey(It.IsAny<IAsymmetricKey>(), It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void WhenKeyIsNotPkcsEncrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted && k.CipherType == CipherType.ElGamal);
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