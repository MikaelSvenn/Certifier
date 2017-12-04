using System.Linq;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;
using Ui.Console.Startup;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class Pkcs8PemWriteFormattingDecoratorTest
    {
        private Pkcs8PemWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommand;
        private Mock<IPemFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncodingWrapper encoding;
        private IAsymmetricKey key;
        private byte[] keyContent;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<IPemFormattingProvider<IAsymmetricKey>>();

            encoding = new EncodingWrapper();
            decorator = new Pkcs8PemWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommand.Object, formattingProvider.Object, encoding);
            
            keyContent = new byte[]{0x07, 0x08, 0x09};
            key = Mock.Of<IAsymmetricKey>(k => k.Content == keyContent);

            formattingProvider.Setup(f => f.GetAsPem(key))
                              .Returns("pemFormattedFoo");
        }

        [Test]
        public void ShouldInvokeDecoratedCommandWithPemFormattedContentWhenContentTypeIsPem()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Pem
            };

            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(encoding.GetBytes("pemFormattedFoo")))));
        }

        [Test]
        public void ShouldInvokeDecoratedCommandWithEmptyContentWhenContentTypeIsNotPem()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Der
            };

            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent == null)));
        }
    }
}