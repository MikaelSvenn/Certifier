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
    public class SshWriteFormattingDecoratorTest
    {
        private SshWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> sshDecorator;
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
            sshDecorator = new SshWriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object, formattingProvider.Object, encoding);

            key = Mock.Of<IAsymmetricKey>(k => k.Content == new byte[] {0x07});
            command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key
            };

            formattingProvider.Setup(fp => fp.GetAsSsh2(key, "converted key"))
                              .Returns("ssh2FormattedKey");

            formattingProvider.Setup(fp => fp.GetAsOpenSsh(key, "converted key"))
                              .Returns("openSshFormattedKey");
        }

        [TestFixture]
        public class WhenContentTypeIsOpenSsh : SshWriteFormattingDecoratorTest
        {
            [Test]
            public void ShouldInvokeDecoratedHandlerWithOpenSshKey()
            {
                command.ContentType = ContentType.OpenSsh;
                sshDecorator.Execute(command);
                decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent.SequenceEqual(encoding.GetBytes("openSshFormattedKey")))));
            }
        }

        [TestFixture]
        public class WhenContentTypeIsSsh2 : SshWriteFormattingDecoratorTest
        {
            [Test]
            public void ShouldInvokeDecoratedHandlerWithSsh2Key()
            {
                command.ContentType = ContentType.Ssh2;
                sshDecorator.Execute(command);
                decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent.SequenceEqual(encoding.GetBytes("ssh2FormattedKey")))));
            }
        }

        [TestFixture]
        public class WhenContentTypeIsNotSsh : SshWriteFormattingDecoratorTest
        {
            [TestCase(ContentType.Der)]
            [TestCase(ContentType.Pem)]
            [TestCase(ContentType.NotSpecified)]
            public void ShouldInvokeDecoratedHandlerWithKeyContent(ContentType contentType)
            {
                command.ContentType = contentType;
                sshDecorator.Execute(command);
                decoratedCommandHandler.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FileContent.SequenceEqual(new byte[]{0x07}))));
            }
        }
    }
}