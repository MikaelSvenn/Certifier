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
    public class EcSec1ValidationDecoratorTest
    {
        private EcSec1ValidationDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommandHandler;
        private IEcKey key;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new EcSec1ValidationDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object);
        }

        [Test]
        public void ShouldThrowWhenSec1IsAppliedForNonEcKey()
        {
            key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Rsa);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };

            Assert.Throws<InvalidOperationException>(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Ec);
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key,
                ContentType = ContentType.Sec1
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(ch => ch.Execute(command));
        }
    }
}