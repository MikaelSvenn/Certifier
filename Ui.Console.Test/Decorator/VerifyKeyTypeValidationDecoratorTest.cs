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
    public class VerifyKeyTypeValidationDecoratorTest
    {
        private VerifyKeyTypeValidationDecorator<VerifyKeyPairCommand> decorator;
        private Mock<ICommandHandler<IVerifyKeyPairCommand>> commandHandler;

        [SetUp]
        public void Setup()
        {
            commandHandler = new Mock<ICommandHandler<IVerifyKeyPairCommand>>();
            decorator = new VerifyKeyTypeValidationDecorator<VerifyKeyPairCommand>(commandHandler.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenPublicKeyAndPrivateKeyAreNotOfSameType()
        {
            var command = new VerifyKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Rsa),
                PublicKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa)
            };

            Assert.Throws<InvalidOperationException>(() => { decorator.Execute(command); });
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = new VerifyKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa),
                PublicKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa)
            };

            Assert.DoesNotThrow(() => {decorator.Execute(command);});
            commandHandler.Verify(c => c.Execute(command));
        }
    }
}