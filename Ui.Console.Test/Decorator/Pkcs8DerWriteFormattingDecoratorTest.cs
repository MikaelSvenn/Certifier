using System.Linq;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class Pkcs8DerWriteFormattingDecoratorTest
    {
        private Pkcs8DerWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommand;
        private IAsymmetricKey key;
        private byte[] keyContent;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new Pkcs8DerWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommand.Object);
            
            keyContent = new byte[]{0x07, 0x08, 0x09};
            key = Mock.Of<IAsymmetricKey>(k => k.Content == keyContent);
        }
        
        [Test]
        public void ShouldInvokeDecoratedCommandWithKeyContentWhenContentTypeIsDer()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Der
            };
        
            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(keyContent))));
        }
        
        [Test]
        public void ShouldInvokeDecoratedCommandWithKeyContentWhenContentTypeIsNotSpecified()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.NotSpecified
            };
        
            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(keyContent))));
        }
    }
}