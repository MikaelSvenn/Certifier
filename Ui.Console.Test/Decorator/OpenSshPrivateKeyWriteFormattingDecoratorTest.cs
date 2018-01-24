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
    public class OpenSshPrivateKeyWriteFormattingDecoratorTest
    {
        private OpenSshPrivateKeyWriteFormattingDecorator<WriteFileCommand<IAsymmetricKeyPair>> openSshPrivateKeyDecorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKeyPair>>> decoratedCommandHandler;
        private Mock<ISshFormattingProvider> formattingProvider;
        private EncodingWrapper encoding;
        private IAsymmetricKeyPair keyPair;
        private WriteFileCommand<IAsymmetricKeyPair> command;
        
        [SetUp]
        public void SetupSshWriteFormattingDecoratorTest()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKeyPair>>>();
            formattingProvider = new Mock<ISshFormattingProvider>();
            encoding = new EncodingWrapper();
            openSshPrivateKeyDecorator = new OpenSshPrivateKeyWriteFormattingDecorator<WriteFileCommand<IAsymmetricKeyPair>>(decoratedCommandHandler.Object, formattingProvider.Object, encoding);

            keyPair = Mock.Of<IAsymmetricKeyPair>();
            command = new WriteFileCommand<IAsymmetricKeyPair>
            {
                Out = keyPair
            };
            
            formattingProvider.Setup(fp => fp.GetAsOpenSshPrivateKey(keyPair, "openssh-key")).Returns("openSshFormattedKey");
        }

        [Test]
        public void ShouldInvokeDecoratedHandlerWithOpenSshKey()
        {
            command.ContentType = ContentType.OpenSsh;
            openSshPrivateKeyDecorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKeyPair>>(c => c.FileContent.SequenceEqual(encoding.GetBytes("openSshFormattedKey")))));
        }
        
        [TestCase(ContentType.Der)]
        [TestCase(ContentType.Pem)]
        [TestCase(ContentType.NotSpecified)]
        [TestCase(ContentType.Ssh2)]
        public void ShouldInvokeDecoratedHandlerWithEmptyContentWhenContentTypeIsNotOpenSsh(ContentType contentType)
        {
            command.ContentType = contentType;
            openSshPrivateKeyDecorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKeyPair>>(c => c.FileContent == null)));
        }
    }
}