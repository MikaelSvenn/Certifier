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
        private Mock<IKeyCommandActivationProvider> keyActivationProvider;
        private Mock<ISignatureCommandActivationProvider> signatureActivationProvider;
        private ApplicationArguments arguments;

        [SetUp]
        public void SetupCommandActivatorTest()
        {
            keyActivationProvider = new Mock<IKeyCommandActivationProvider>();
            signatureActivationProvider = new Mock<ISignatureCommandActivationProvider>();
            activator = new CommandActivator(keyActivationProvider.Object, signatureActivationProvider.Object);
            arguments = new ApplicationArguments();
        }

        [TestFixture]
        public class Create : CommandActivatorTest
        {
            [Test]
            public void NoneShouldThrowException()
            {
                Assert.Throws<InvalidOperationException>(() => { activator.Create[OperationTarget.None](arguments); });
            }

            [Test]
            public void KeyShouldInvokeCreateKey()
            {
                activator.Create[OperationTarget.Key](arguments);
                keyActivationProvider.Verify(ap => ap.CreateKeyPair(arguments));
            }

            [Test]
            public void SignatureShouldInvokeCreateSignature()
            {
                activator.Create[OperationTarget.Signature](arguments);
                signatureActivationProvider.Verify(ap => ap.CreateSignature(arguments));
            }
        }

        [TestFixture]
        public class Verify : CommandActivatorTest
        {
            [Test]
            public void NoneShouldThrowException()
            {
                Assert.Throws<InvalidOperationException>(() => { activator.Verify[OperationTarget.None](arguments); });
            }

            [Test]
            public void KeyShouldInvokeVerifyKeyPair()
            {
                activator.Verify[OperationTarget.Key](arguments);
                keyActivationProvider.Verify(ap => ap.VerifyKeyPair(arguments));
            }

            [Test]
            public void SignatureShouldInvokeVerifySignature()
            {
                activator.Verify[OperationTarget.Signature](arguments);
                signatureActivationProvider.Verify(ap => ap.VerifySignature(arguments));
            }
        }
    }
}