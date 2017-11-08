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
    public class Ssh2WriteFormattingDecoratorTest
    {
        private Ssh2WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> ssh2Decorator;
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
            ssh2Decorator = new Ssh2WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object, formattingProvider.Object, encoding);

            key = Mock.Of<IAsymmetricKey>(k => k.Content == new byte[] {0x07});
            command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key
            };

            formattingProvider.Setup(fp => fp.GetAsSsh2(key, "ssh2-key")).Returns("ssh2FormattedKey");
        }

        [Test]
        public void ShouldInvokeDecoratedHandlerWithSsh2Key()
        {
            command.ContentType = ContentType.Ssh2;
            ssh2Decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent.SequenceEqual(encoding.GetBytes("ssh2FormattedKey")))));
        }
        
        [TestCase(ContentType.Der)]
        [TestCase(ContentType.Pem)]
        [TestCase(ContentType.NotSpecified)]
        [TestCase(ContentType.OpenSsh)]
        public void ShouldInvokeDecoratedHandlerWithEmptyContentWhenContentTypeIsNotOpenSsh(ContentType contentType)
        {
            command.ContentType = contentType;
            ssh2Decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent == null)));
        }
    }
}