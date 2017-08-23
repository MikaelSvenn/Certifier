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
    public class RsaKeySizeValidationDecoratorTest
    {
        private RsaKeySizeValidationDecorator<CreateKeyCommand<RsaKey>> decorator;
        private Mock<ICommandHandler<CreateKeyCommand<RsaKey>>> decoratedCommand;

        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<CreateKeyCommand<RsaKey>>>();
            decorator = new RsaKeySizeValidationDecorator<CreateKeyCommand<RsaKey>>(decoratedCommand.Object);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenKeySizeIs2048()
        {
            var command = new CreateKeyCommand<RsaKey>
            {
                KeySize = 2048
            };

            Assert.DoesNotThrow(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldNotThrowExceptionWhenKeySizeIsOver2048()
        {
            var command = new CreateKeyCommand<RsaKey>
            {
                KeySize = 9999
            };

            Assert.DoesNotThrow(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldInvokeDecoratedCommand()
        {
            var command = new CreateKeyCommand<RsaKey>
            {
                KeySize = 2048
            };

            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenKeySizeIsLessThan2048()
        {
            var command = new CreateKeyCommand<RsaKey>
            {
                KeySize = 1024
            };

            Assert.Throws<ArgumentException>(() => decorator.Execute(command));
        }
    }
}