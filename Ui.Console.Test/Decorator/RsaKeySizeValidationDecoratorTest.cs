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
        private RsaKeySizeValidationDecorator<CreateRsaKeyCommand> decorator;
        private Mock<ICommandHandler<ICreateAsymmetricKeyCommand>> decoratedCommand;

        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<ICreateAsymmetricKeyCommand>>();
            decorator = new RsaKeySizeValidationDecorator<CreateRsaKeyCommand>(decoratedCommand.Object);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenKeySizeIs2048()
        {
            var command = new CreateRsaKeyCommand
            {
                KeySize = 2048
            };

            Assert.DoesNotThrow(() => decorator.Excecute(command));
        }

        [Test]
        public void ShouldNotThrowExceptionWhenKeySizeIsOver2048()
        {
            var command = new CreateRsaKeyCommand
            {
                KeySize = 9999
            };

            Assert.DoesNotThrow(() => decorator.Excecute(command));
        }

        [Test]
        public void ShouldInvokeDecoratedCommand()
        {
            var command = new CreateRsaKeyCommand
            {
                KeySize = 2048
            };

            decorator.Excecute(command);
            decoratedCommand.Verify(d => d.Excecute(command));
        }

        [Test]
        public void ShouldThrowExceptionWhenKeySizeIsLessThan2048()
        {
            var command = new CreateRsaKeyCommand
            {
                KeySize = 1024
            };

            Assert.Throws<ArgumentException>(() => decorator.Excecute(command));
        }
    }
}