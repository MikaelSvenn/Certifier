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
        private Mock<Base64Wrapper> base64;

        [OneTimeSetUp]
        public void SetupCommandActivationProviderTest()
        {
            commandExecutor = new Mock<ICommandExecutor>();
            encoding = new Mock<EncodingWrapper>();
            base64 = new Mock<Base64Wrapper>();
            provider = new CommandActivationProvider(commandExecutor.Object, new RsaKeyCommandProvider(), new FileCommandProvider(), new SignatureCommandProvider(), encoding.Object, base64.Object);
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
        
        [TestFixture]
        public class VerifySignature : CommandActivationProviderTest
        {
            private IAsymmetricKey publicKey;
            private byte[] contentFromFile;
            private byte[] contentFromUserInput;
            private byte[] signatureFromFile;
            private byte[] decodedSignatureFromFile;
            private byte[] signatureFromUserInput;

            [SetUp]
            public void SetupVerifySignature()
            {
                publicKey = Mock.Of<IAsymmetricKey>();
                
                contentFromFile = new byte[]{0x07, 0x08};
                contentFromUserInput = new byte[]{0x09, 0x10};
                signatureFromFile = new byte[]{0x11, 0x12};
                decodedSignatureFromFile = new byte[]{0x13, 0x14};
                signatureFromUserInput = new byte[]{0x15, 0x16};
                
                encoding.Setup(e => e.GetBytes("userInput"))
                        .Returns(contentFromUserInput);

                encoding.Setup(e => e.GetBytes("userSignature"))
                        .Returns(signatureFromUserInput);

                encoding.Setup(e => e.GetString(signatureFromFile))
                        .Returns("base64EncodedSignatureFromFile");
                               
                base64.Setup(b => b.IsBase64("base64EncodedSignature"))
                      .Returns(true);
                
                base64.Setup(b => b.FromBase64String("base64EncodedSignature"))
                      .Returns(signatureFromUserInput);

                base64.Setup(b => b.FromBase64String("base64EncodedSignatureFromFile"))
                      .Returns(decodedSignatureFromFile);
                
                commandExecutor.Setup(c => c.Execute(It.IsAny<object>()))
                               .Callback<object>(c =>
                               {
                                   var fileCommand = c as ReadFileCommand<byte[]>;
                                   if (fileCommand != null && fileCommand.FilePath == "foo.file")
                                   {
                                       fileCommand.Result = contentFromFile;
                                   }
                                   if (fileCommand != null && fileCommand.FilePath == "path/to/signature")
                                   {
                                       fileCommand.Result = signatureFromFile;
                                   }
                                   
                                   var keyCommand = c as ReadKeyFromFileCommand;
                                   if (keyCommand != null)
                                   {
                                       keyCommand.Result = publicKey;
                                   }
                               });
                
                arguments = new ApplicationArguments
                {
                    PublicKeyPath = "foopath"
                };
            }

            [TearDown]
            public void Teardown()
            {
                commandExecutor.ResetCalls();
            }
            
            [Test]
            public void ShouldReadPublicKey()
            {
                provider.VerifySignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(rc => rc.FilePath == "foopath")));
            }

            [Test]
            public void ShouldVerifySignatureWithGivenPublicKey()
            {
                provider.VerifySignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<VerifySignatureCommand>(c => c.PublicKey == publicKey)));
            }
            
            [TestFixture]
            public class WhenVerifyingSignatureForFileContent : VerifySignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.FileInput = "foo.file";
                    provider.VerifySignature(arguments);
                }
                
                [Test]
                public void ShouldReadFileToVerify()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadFileCommand<byte[]>>(c => c.FilePath == "foo.file")));
                }

                [Test]
                public void ShouldVerifySignatureForGivenFile()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<VerifySignatureCommand>(c => c.Signature.SignedData == contentFromFile)));
                }
            }

            [TestFixture]
            public class WhenVerifyingSignatureForUserInput : VerifySignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.Input = "userInput";
                    provider.VerifySignature(arguments);
                }
                
                [Test]
                public void ShouldVerifySignatureForGivenInput()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<VerifySignatureCommand>(c => c.Signature.SignedData == contentFromUserInput)));
                }

                [Test]
                public void ShouldNotReadFileToVerify()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadFileCommand<byte[]>>(c => c.FilePath == "userInput" || c.FilePath == string.Empty)), Times.Never);
                }
            }

            [TestFixture]
            public class WhenGivenSignatureIsBase64Encoded : VerifySignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.Signature = "base64EncodedSignature";
                    provider.VerifySignature(arguments);
                }
                
                [Test]
                public void ShouldVerifyGivenSignature()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<VerifySignatureCommand>(c => c.Signature.Content == signatureFromUserInput)));
                }

                [Test]
                public void ShouldNotReadSignatureFromFile()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadFileCommand<byte[]>>(c => c.FilePath == "base64EncodedSignature")), Times.Never);
                }
            }

            [TestFixture]
            public class WhenGivenSignatureIsNotBase64Encoded : VerifySignature
            {
                [SetUp]
                public void Setup()
                {
                    arguments.Signature = "path/to/signature";
                    provider.VerifySignature(arguments);
                }
                
                [Test]
                public void ShouldReadSignatureFromFile()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadFileCommand<byte[]>>(c => c.FilePath == "path/to/signature")));
                }

                [Test]
                public void ShouldVerifyGivenSignature()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<VerifySignatureCommand>(c => c.Signature.Content == decodedSignatureFromFile)));
                }
            }
        }
    }
}