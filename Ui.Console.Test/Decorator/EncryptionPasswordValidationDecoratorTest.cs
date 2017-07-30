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
    public class EncryptionPasswordValidationDecoratorTest
    {
        private EncryptionPasswordValidationDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommand;

        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new EncryptionPasswordValidationDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommand.Object);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenCommandHasPassword()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.Password == "foo");
            Assert.DoesNotThrow(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldInvokeDecoratedCommand()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.Password == "foo");
            decorator.Execute(command);

            decoratedCommand.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenEncryptionIsSpecifiedAndNoPasswordIsGiven()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.Pkcs);
            Assert.Throws<ArgumentException>(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenEncryptionIsSpecifiedAndEmptyPasswordIsGiven()
        {
            var command = Mock.Of<WriteFileCommand<IAsymmetricKey>>(c => c.EncryptionType == EncryptionType.Pkcs && c.Password == "");
            Assert.Throws<ArgumentException>(() => decorator.Execute(command));
        }
    }
}