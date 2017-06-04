using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class CommandActivationProviderTest
    {
        private CommandActivationProvider provider;
        private Mock<ICommandExecutor> commandExecutor;
        private ApplicationArguments arguments;
        private Mock<EncodingWrapper> encoding;

        [OneTimeSetUp]
        public void SetupCommandActivationProviderTest()
        {
            commandExecutor = new Mock<ICommandExecutor>();
            encoding = new Mock<EncodingWrapper>();
            provider = new CommandActivationProvider(commandExecutor.Object, new RsaKeyCommandProvider(), new FileCommandProvider(), new SignatureCommandProvider(), encoding.Object);
        }

        [TestFixture]
        public class CreateKey : CommandActivationProviderTest
        {
            private IAsymmetricKey privateKey;
            private IAsymmetricKey publicKey;

            [OneTimeSetUp]
            public void Setup()
            {
                privateKey = Mock.Of<IAsymmetricKey>();
                publicKey = Mock.Of<IAsymmetricKey>();

                var createdKeyPair = new AsymmetricKeyPair(privateKey, publicKey);
                commandExecutor.Setup(c => c.Execute(It.IsAny<object>()))
                    .Callback<object>(c => ((ICommandWithResult<IAsymmetricKeyPair>)c).Result = createdKeyPair);

                arguments = new ApplicationArguments
                {
                    KeySize = 1024,
                    EncryptionType = KeyEncryptionType.None,
                    PrivateKeyPath = "private.pem",
                    PublicKeyPath = "public.pem"
                };

                provider.CreateKey(arguments);
            }

            [Test]
            public void ShouldCreateRsaKeyPair()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateRsaKeyCommand>(c => c.EncryptionType == KeyEncryptionType.None && c.KeySize == 1024)));
            }

            [Test]
            public void ShouldWriteCreatedPrivateKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.First().Out == privateKey && 
                                                                                                                          w.First().FilePath == "private.pem")));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.Last().Out == publicKey && 
                                                                                                                          w.Last().FilePath == "public.pem")));
            }
        }

        [TestFixture]
        public class CreateSignature : CommandActivationProviderTest
        {
            private IAsymmetricKey key;
            private Signature signature;
            private byte[] fileContent;
            
            [SetUp]
            public void SetupCreateSignature()
            {
                key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                arguments = new ApplicationArguments
                {
                    PrivateKeyPath = "private.pem",
                    Password = "kensentme",
                    Input = "FooBarBaz"
                };
                
                fileContent = new byte[] { 0x07 };
                signature = new Signature();
                
                commandExecutor.Setup(c => c.Execute(It.IsAny<object>()))
                               .Callback<object>(c =>
                               {
                                   var fileCommand = c as ReadFileCommand<byte[]>;
                                   if (fileCommand != null)
                                   {
                                       fileCommand.Result = fileContent;    
                                   }

                                   var keyCommand = c as ReadKeyFromFileCommand;
                                   if (keyCommand != null)
                                   {
                                       keyCommand.Result = key;
                                   }
                                   
                                   var signatureCommand = c as CreateSignatureCommand;
                                   if (signatureCommand != null)
                                   {
                                       signatureCommand.Result = signature;
                                   }
                               });
            }

            [Test]
            public void ShouldReadPrivateKey()
            {
                provider.CreateSignature(arguments);                
                commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => c.FilePath == "private.pem" && c.Password == "kensentme")));
            }
            
            [TestFixture]
            public class WhenSigningFile : CreateSignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.Input = string.Empty;
                    arguments.FileInput = "file";
                    arguments.FileOutput = "file.signature";
                }
                
                [Test]
                public void ShouldReadFileToBeSigned()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadFileCommand<byte[]>>(c => c.FilePath == "file")));
                }
                
                [Test]
                public void ShouldCreateSignatureForGivenFileWithGivenKey()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<CreateSignatureCommand>(c => c.PrivateKey == key && c.ContentToSign == fileContent)));
                }
            }

            [TestFixture]
            public class WhenSigningUserInput : CreateSignature
            {
                private byte[] userInput;
                
                [SetUp]
                public void Setup()
                {
                    userInput = new byte[] {0x08};
                    encoding.Setup(e => e.GetBytes("FooBarBaz"))
                            .Returns(userInput);
                }
                
                [Test]
                public void ShouldNotReadFile()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.IsAny<ReadFileCommand<byte[]>>()), Times.Never);
                }
                
                [Test]
                public void ShouldCreateSignatureForGivenUserInputWithGivenKey()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<CreateSignatureCommand>(c => c.PrivateKey == key && c.ContentToSign == userInput)));
                }
            }

            [TestFixture]
            public class WhenWritingSignatureToFile : CreateSignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.FileOutput = "file.signature";
                }
                
                [Test]
                public void ShouldWriteCreatedSignatureToFile()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<Signature>>(c => c.Out == signature && c.FilePath == "file.signature")));
                }
                
                [Test]
                public void ShouldNotWriteCreatedSignatureToStdOut()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteToStdOutCommand<Signature>>(c => c.Out == signature)), Times.Never());
                }
            }

            [TestFixture]
            public class WhenWritingSignatureToStandardOutput : CreateSignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.FileOutput = string.Empty;
                }
                
                [Test]
                public void ShouldNotWriteCreatedSignatureToFile()
                {                    
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<Signature>>(c => c.Out == signature && c.FilePath == "file.signature")), Times.Never());
                }
                
                [Test]
                public void ShouldWriteCreatedSignatureToStdOut()
                {
                    provider.CreateSignature(arguments);
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteToStdOutCommand<Signature>>(c => c.Out == signature)));
                }
            }            
        }
    }
}