using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class ReadKeyConversionDecoratorTest
    {
        private ReadKeyConversionDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;
        private Mock<IAsymmetricKeyProvider> asymmetricKeyProvider;
        private IAsymmetricKey privateKey;
        private IAsymmetricKey encryptedPrivateKey;
        private IAsymmetricKey publicKey;
        
        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();

            privateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
            encryptedPrivateKey = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted);
            publicKey = Mock.Of<IAsymmetricKey>();
            
            asymmetricKeyProvider = new Mock<IAsymmetricKeyProvider>();
            asymmetricKeyProvider.Setup(akp => akp.GetPrivateKey(It.IsAny<byte[]>()))
                                 .Returns(privateKey);
            asymmetricKeyProvider.Setup(akp => akp.GetEncryptedPrivateKey(It.IsAny<byte[]>()))
                                 .Returns(encryptedPrivateKey);
            asymmetricKeyProvider.Setup(akp => akp.GetPublicKey(It.IsAny<byte[]>()))
                                 .Returns(publicKey);
            
            
            decorator = new ReadKeyConversionDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object, asymmetricKeyProvider.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = new ReadKeyFromFileCommand();
            decorator.Execute(command);
            
            decoratedHandler.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldNotSetResultWhenItExists()
        {
            var key = Mock.Of<IAsymmetricKey>();
            var command = new ReadKeyFromFileCommand
            {
                Result = key
            };
            
            decorator.Execute(command);
            Assert.AreSame(key, command.Result);
        }

        [Test]
        public void ShouldSetPrivateKeyAsResultWhenPrivateKeyIsRead()
        {
            var command = new ReadKeyFromFileCommand
            {
                IsPrivateKey = true
            };
            
            decorator.Execute(command);
            Assert.AreSame(privateKey, command.Result);
        }

        [Test]
        public void ShouldSetEncryptedPrivateKeyAsResultWhenPasswordIsProvidedForPrivateKey()
        {
            var command = new ReadKeyFromFileCommand
            {
                IsPrivateKey = true,
                Password = "foo"
            };
            
            decorator.Execute(command);
            Assert.AreSame(encryptedPrivateKey, command.Result);
        }

        [Test]
        public void ShouldSetPublicKeyAsResultWhenWhenPublicKeyIsRead()
        {
            var command = new ReadKeyFromFileCommand();
            
            decorator.Execute(command);
            Assert.AreSame(publicKey, command.Result);
        }
    }
}