using System;
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
    public class OpenSshKeyEncryptionDecoratorTest
    {
        private OpenSshKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKeyPair>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKeyPair>>> decorated;

        [SetUp]
        public void SetupAesKeyEncryptionDecoratorTest()
        {
            decorated = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKeyPair>>>();
            decorator = new OpenSshKeyEncryptionDecorator<WriteFileCommand<IAsymmetricKeyPair>>(decorated.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKeyPair>>(k => k.ContentType == ContentType.OpenSsh);
            decorator.Execute(command);
            decorated.Verify(d => d.Execute(command));
        }

        [TestCase(ContentType.Der)]
        [TestCase(ContentType.NotSpecified)]
        [TestCase(ContentType.Pem)]
        [TestCase(ContentType.Sec1)]
        [TestCase(ContentType.Ssh2)]
        public void ShouldNotThrowWhenEncryptionIsAppliedAndContentTypeIsNotOpenSsh(ContentType contentType)
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKeyPair>>(k => k.ContentType == contentType &&
                                                                             k.EncryptionType == EncryptionType.Aes);

            Assert.DoesNotThrow(() => decorator.Execute(command));
        }

        [TestCase(ContentType.Der)]
        [TestCase(ContentType.OpenSsh)]
        [TestCase(ContentType.NotSpecified)]
        [TestCase(ContentType.Pem)]
        [TestCase(ContentType.Sec1)]
        [TestCase(ContentType.Ssh2)]
        public void ShouldNotThrowWhenEncryptionIsNotApplied(ContentType contentType)
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKeyPair>>(k => k.ContentType == contentType &&
                                                                             k.EncryptionType == EncryptionType.None);

            Assert.DoesNotThrow(() => decorator.Execute(command));
        }
        
        [TestCase(EncryptionType.Aes)]
        [TestCase(EncryptionType.Pkcs)]
        public void ShouldThrowWhenEncryptionIsAppliedAndContentTypeIsOpenSsh(EncryptionType encryptionType)
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKeyPair>>(k => k.ContentType == ContentType.OpenSsh &&
                                                                             k.EncryptionType == encryptionType);

            Assert.Throws<InvalidOperationException>(() => decorator.Execute(command));
        }
    }
}