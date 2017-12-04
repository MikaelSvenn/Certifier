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
    public class EcSec1PemWriteFormattingDecoratorTest
    {
        private EcSec1PemWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommandHandler;
        private Mock<IPemFormattingProvider<IEcKey>> formattingProvider;
        private EncodingWrapper encoding;
        private IEcKey key;
        
        [SetUp]
        public void Setup()
        {
            formattingProvider = new Mock<IPemFormattingProvider<IEcKey>>();
            encoding = new EncodingWrapper();
            
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new EcSec1PemWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object, formattingProvider.Object, encoding);

            key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Ec);
            formattingProvider.Setup(fp => fp.GetAsPem(key))
                              .Returns("EcPem");
        }

        [Test]
        public void ShouldSetFileContentWhenContentTypeIsSec1AndCipherTypeIsEc()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };

            decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(encoding.GetBytes("EcPem")))));
        }

        [Test]
        public void ShouldNotSetFileContentWhenCipherTypeIsNotEc()
        {
            key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Rsa);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };

            decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent == null)));
        }
        
        [Test]
        public void ShouldNotSetFileContentWhenContentTypeIsNotPem()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Ssh2
            };

            decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent == null)));
        }
    }
}