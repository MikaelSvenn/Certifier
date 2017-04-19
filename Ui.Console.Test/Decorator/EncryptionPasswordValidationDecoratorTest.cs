using System;
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
        private EncryptionPasswordValidationDecorator<ICreateAsymmetricKeyCommand> decorator;
        private Mock<ICommandHandler<ICreateAsymmetricKeyCommand>> decoratedCommand;

        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<ICreateAsymmetricKeyCommand>>();
            decorator = new EncryptionPasswordValidationDecorator<ICreateAsymmetricKeyCommand>(decoratedCommand.Object);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenCommandHasPassword()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.Password == "foo");
            Assert.DoesNotThrow(() => decorator.Excecute(command));
        }

        [Test]
        public void ShouldInvokeDecoratedCommand()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.Password == "foo");
            decorator.Excecute(command);

            decoratedCommand.Verify(d => d.Excecute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenEncryptionIsSpecifiedAndNoPasswordIsGiven()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.EncryptionType == KeyEncryptionType.Pkcs);
            Assert.Throws<ArgumentException>(() => decorator.Excecute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenEncryptionIsSpecifiedAndEmptyPasswordIsGiven()
        {
            var command = Mock.Of<ICreateAsymmetricKeyCommand>(c => c.EncryptionType == KeyEncryptionType.Pkcs && c.Password == "");
            Assert.Throws<ArgumentException>(() => decorator.Excecute(command));
        }
    }
}