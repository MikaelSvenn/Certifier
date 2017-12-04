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
    public class EcSec1WriteFormattingDecoratorTest
    {
        private EcSec1WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommandHandler;
        private Mock<IEcKeyProvider> keyProvider;
        private IEcKey formattedKey;
     
        [SetUp]
        public void Setup()
        {
            formattedKey = Mock.Of<IEcKey>();
            keyProvider = new Mock<IEcKeyProvider>();
            keyProvider.Setup(kp => kp.GetPkcs8PrivateKeyAsSec1(It.IsAny<IEcKey>()))
                       .Returns(formattedKey);
            
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new EcSec1WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object, keyProvider.Object);
        }

        [Test]
        public void ShouldSetSec1FormattedPrivateKeyContentToCommand()
        {
            var key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Ec && k.IsPrivateKey);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dc => dc.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.Out == formattedKey)));
        }

        [Test]
        public void ShouldNotSetSec1ContentWhenCipherTypeIsNotEc()
        {
            var key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa && k.IsPrivateKey);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dc => dc.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.Out == key)));
        }

        [Test]
        public void ShouldNotSetSec1ContentWhenKeyIsNotPrivateKey()
        {
            var key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Ec && !k.IsPrivateKey);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dc => dc.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.Out == key)));   
        }

        [Test]
        public void ShouldNotSetSec1ContentWhenContentTypeIsNotSec1()
        {
            var key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Ec && k.IsPrivateKey);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Pem
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dc => dc.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.Out == key)));  
        }
    }
}