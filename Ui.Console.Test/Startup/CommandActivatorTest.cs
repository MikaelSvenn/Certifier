using System;
using Moq;
using NUnit.Framework;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Startup
{
    [TestFixture]
    public class CommandActivatorTest
    {
        private CommandActivator activator;
        private Mock<ICommandActivationProvider> activationProvider;
        private ApplicationArguments arguments;

        [SetUp]
        public void SetupCommandActivatorTest()
        {
            activationProvider = new Mock<ICommandActivationProvider>();
            activator = new CommandActivator(activationProvider.Object);
            arguments = new ApplicationArguments();
        }

        [TestFixture]
        public class Create : CommandActivatorTest
        {
            [Test]
            public void NoneShouldThrowException()
            {
                Assert.Throws<InvalidOperationException>(() => { activator.Create[OperationTarget.none](arguments); });
            }

            [Test]
            public void KeyShouldInvokeCreateKey()
            {
                activator.Create[OperationTarget.key](arguments);
                activationProvider.Verify(ap => ap.CreateKey(arguments));
            }

            [Test]
            public void SignatureShouldInvokeCreateSignature()
            {
                activator.Create[OperationTarget.signature](arguments);
                activationProvider.Verify(ap => ap.CreateSignature(arguments));
            }
        }

        [TestFixture]
        public class Verify : CommandActivatorTest
        {
            [Test]
            public void NoneShouldThrowException()
            {
                Assert.Throws<InvalidOperationException>(() => { activator.Verify[OperationTarget.none](arguments); });
            }

            [Test]
            public void KeyShouldInvokeVerifyKeyPair()
            {
                activator.Verify[OperationTarget.key](arguments);
                activationProvider.Verify(ap => ap.VerifyKeyPair(arguments));
            }

            [Test]
            public void SignatureShouldInvokeVerifySignature()
            {
                activator.Verify[OperationTarget.signature](arguments);
                activationProvider.Verify(ap => ap.VerifySignature(arguments));
            }
        }
    }
}