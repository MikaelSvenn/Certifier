using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class VerifySignatureCommandHandlerTest
    {
        private VerifySignatureCommandHandler commandHandler;
        private Mock<ISignatureProvider> signatureProvider;
        private Signature signature;
        private IAsymmetricKey publicKey;
        
        [SetUp]
        public void Setup()
        {
            signatureProvider = new Mock<ISignatureProvider>();
            commandHandler = new VerifySignatureCommandHandler(signatureProvider.Object);
            signature = Mock.Of<Signature>();
            publicKey = Mock.Of<IAsymmetricKey>();

            signatureProvider.Setup(s => s.VerifySignature(publicKey, signature))
                             .Returns(true);
        }

        [Test]
        public void ShouldVerifyGivenSignatureWithGivenKey()
        {
            commandHandler.Execute(new VerifySignatureCommand
            {
                PublicKey = publicKey,
                Signature = signature
            });
            
            signatureProvider.Verify(s => s.VerifySignature(publicKey, signature));
        }

        [Test]
        public void ShouldThrowExceptionWhenSignatureIsNotValid()
        {
            signatureProvider.Setup(s => s.VerifySignature(publicKey, signature))
                             .Returns(false);

            Assert.Throws<CryptographicException>(() => commandHandler.Execute(new VerifySignatureCommand
            {
                PublicKey = publicKey,
                Signature = signature
            }));
        }
    }
}