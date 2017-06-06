using System.Security.Policy;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class SignatureCommandProviderTest
    {
        private SignatureCommandProvider commandProvider;

        [SetUp]
        public void SetupSignatureCommandProviderTest()
        {
            commandProvider = new SignatureCommandProvider();
        }

        [TestFixture]
        public class GetCreateSignatureCommand : SignatureCommandProviderTest
        {
            private CreateSignatureCommand command;
            private IAsymmetricKey key;
            private byte[] content;

            [SetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>();
                content = new byte[]{ 0x07 };

                command = commandProvider.GetCreateSignatureCommand(key, content);
            }

            [Test]
            public void ShouldMapPrivateKey()
            {
                Assert.AreEqual(key, command.PrivateKey);
            }

            [Test]
            public void ShouldMapContentToSign()
            {
                Assert.AreEqual(content, command.ContentToSign);
            }
        }
        
        [TestFixture]
        public class GetVerifySignatureCommand : SignatureCommandProviderTest
        {
            private VerifySignatureCommand result;
            private IAsymmetricKey publicKey;
            private byte[] signedContent;
            private byte[] signature;
            
            [SetUp]
            public void Setup()
            {
                publicKey = Mock.Of<IAsymmetricKey>();
                signedContent = new byte[]{0x10};
                signature = new byte[]{0x20};

                result = commandProvider.GetVerifySignatureCommand(publicKey, signedContent, signature);
            }

            [Test]
            public void ShouldMapPublicKey()
            {
                Assert.AreEqual(publicKey, result.PublicKey);                
            }

            [Test]
            public void ShouldMapSignedContent()
            {
                Assert.AreEqual(signedContent, result.Signature.SignedData);   
            }

            [Test]
            public void ShouldMapSignature()
            {
                Assert.AreEqual(signature, result.Signature.Content);
            }
        }
    }
}