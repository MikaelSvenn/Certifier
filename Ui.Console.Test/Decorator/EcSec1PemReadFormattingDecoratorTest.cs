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
    public class EcSec1PemReadFormattingDecoratorTest
    {
        private EcSec1PemReadFormattingDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedCommandHandler;
        private Mock<IPemFormattingProvider<IEcKey>> sec1PemFormatter;
        private IEcKey convertedKey;
        private EncodingWrapper encoding;
        
        [SetUp]
        public void Setup()
        {
            convertedKey = Mock.Of<IEcKey>();
            sec1PemFormatter = new Mock<IPemFormattingProvider<IEcKey>>();
            sec1PemFormatter.Setup(s => s.GetAsDer("-----BEGIN EC PRIVATE KEY given content"))
                            .Returns(convertedKey);
            
            encoding = new EncodingWrapper();
            decoratedCommandHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            decorator = new EcSec1PemReadFormattingDecorator<ReadKeyFromFileCommand>(decoratedCommandHandler.Object, sec1PemFormatter.Object, encoding);
        }

        [Test]
        public void ShouldInvokeDecoratedCommandHandler()
        {
            var command = new ReadKeyFromFileCommand
            {
                Result = Mock.Of<IEcKey>()
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dch => dch.Execute(command));
        }
        
        [Test]
        public void ShouldSetResult()
        {
            var command = new ReadKeyFromFileCommand
            {
                FileContent = encoding.GetBytes("-----BEGIN EC PRIVATE KEY given content")
            };
            
            decorator.Execute(command);
            Assert.AreEqual(convertedKey, command.Result);
        }

        [Test]
        public void ShouldSetOriginalContentType()
        {
            var command = new ReadKeyFromFileCommand
            {
                FileContent = encoding.GetBytes("-----BEGIN EC PRIVATE KEY given content")
            };
            
            decorator.Execute(command);
            Assert.AreEqual(ContentType.Sec1, command.OriginalContentType);
        }

        [Test]
        public void ShouldNotSetResultWhenKeyIsNotEcSec1Formatted()
        {
            var command = new ReadKeyFromFileCommand
            {
                FileContent = encoding.GetBytes("-----BEGIN PRIVATE KEY given content")
            };
            
            decorator.Execute(command);
            Assert.IsNull(command.Result);
        }

        [Test]
        public void ShouldNotSetOriginalContentTypeWhenKeyIsNotEcSec1Formatted()
        {
            var command = new ReadKeyFromFileCommand
            {
                FileContent = encoding.GetBytes("-----BEGIN PRIVATE KEY given content"),
                OriginalContentType = ContentType.Der
            };
            
            decorator.Execute(command);
            Assert.AreEqual(ContentType.Der, command.OriginalContentType);
        }
    }
}