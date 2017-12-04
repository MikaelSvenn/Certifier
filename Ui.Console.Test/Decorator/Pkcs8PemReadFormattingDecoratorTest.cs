using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class Pkcs8PemReadFormattingDecoratorTest
    {
        private Pkcs8PemReadFormattingDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;
        private Mock<IPemFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncryptedKey resultingKey;
        private ReadKeyFromFileCommand command;
        private EncodingWrapper encoding;
        
        [SetUp]
        public void Setup()
        {
            encoding = new EncodingWrapper();
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            formattingProvider = new Mock<IPemFormattingProvider<IAsymmetricKey>>();
            decorator = new Pkcs8PemReadFormattingDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object, formattingProvider.Object, encoding);

            resultingKey = new EncryptedKey(null, CipherType.Pkcs12Encrypted);
            
            formattingProvider.Setup(fp => fp.GetAsDer("-----BEGIN PRIVATE fileContent"))
                              .Returns(resultingKey);
            formattingProvider.Setup(fp => fp.GetAsDer("-----BEGIN PUBLIC fileContent"))
                              .Returns(resultingKey);
            formattingProvider.Setup(fp => fp.GetAsDer("-----BEGIN ENCRYPTED fileContent"))
                              .Returns(resultingKey);
            
            command = new ReadKeyFromFileCommand();
            
        }

        [TestCase("-----BEGIN PRIVATE fileContent")]
        [TestCase("-----BEGIN PUBLIC fileContent")]
        [TestCase("-----BEGIN ENCRYPTED fileContent")]
        public void ShouldSetDerFormattedContentAsCommandResult(string content)
        {
            decoratedHandler.Setup(dh => dh.Execute(command))
                             .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes(content));
            
            decorator.Execute(command);
            Assert.AreEqual(resultingKey, command.Result);
        }

        [TestCase("-----BEGIN PRIVATE fileContent")]
        [TestCase("-----BEGIN PUBLIC fileContent")]
        [TestCase("-----BEGIN ENCRYPTED fileContent")]
        public void ShouldSetOriginalContentType(string content)
        {
            decoratedHandler.Setup(dh => dh.Execute(command))
                             .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes(content));
            
            decorator.Execute(command);
            Assert.AreEqual(ContentType.Pem, command.OriginalContentType);
        }
        
        [TestCase("-----BEGIN EC PRIVATE fileContent")]
        [TestCase("foobar")]
        [TestCase("4")]
        public void ShouldNotSetContentWhenContentIsNotPkcs8PemFormatted(string content)
        {
            decoratedHandler.Setup(dh => dh.Execute(command))
                            .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes(content));

            decorator.Execute(command);
            Assert.IsNull(command.Result);
        }
        
        [TestCase("-----BEGIN EC PRIVATE fileContent")]
        [TestCase("foobar")]
        [TestCase("4")]
        public void ShouldNotSetOriginalContentTypeWhenContentIsNotPkcs8PemFormatted(string content)
        {
            decoratedHandler.Setup(dh => dh.Execute(command))
                             .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes(content));

            command.OriginalContentType = ContentType.Ssh2;
            decorator.Execute(command);
            Assert.AreEqual(ContentType.Ssh2, command.OriginalContentType);
        }
    }
}