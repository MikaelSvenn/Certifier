using System;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class OpenSshWriteFormattingDecoratorTest
    {
        private OpenSshWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> openSshDecorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommandHandler;
        private Mock<ISshFormattingProvider> formattingProvider;
        private EncodingWrapper encoding;
        private IAsymmetricKey key;
        private WriteFileCommand<IAsymmetricKey> command;
        
        [SetUp]
        public void SetupSshWriteFormattingDecoratorTest()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<ISshFormattingProvider>();
            encoding = new EncodingWrapper();
            openSshDecorator = new OpenSshWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object, formattingProvider.Object, encoding);

            key = Mock.Of<IAsymmetricKey>(k => k.Content == new byte[] {0x07});
            command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key
            };

            formattingProvider.Setup(fp => fp.GetAsOpenSsh(key, "openssh-key")).Returns("openSshFormattedKey");
        }

        [Test]
        public void ShouldInvokeDecoratedHandlerWithOpenSshKey()
        {
            command.ContentType = ContentType.OpenSsh;
            openSshDecorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent.SequenceEqual(encoding.GetBytes("openSshFormattedKey")))));
        }
        
        [TestCase(ContentType.Der)]
        [TestCase(ContentType.Pem)]
        [TestCase(ContentType.NotSpecified)]
        [TestCase(ContentType.Ssh2)]
        public void ShouldInvokeDecoratedHandlerWithEmptyContentWhenContentTypeIsNotOpenSsh(ContentType contentType)
        {
            command.ContentType = contentType;
            openSshDecorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent == null)));
        }
    }
}