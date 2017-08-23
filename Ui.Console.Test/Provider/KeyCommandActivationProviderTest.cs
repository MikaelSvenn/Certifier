using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class KeyCommandActivationProviderTest
    {
        private KeyCommandActivationProvider provider;
        private Mock<ICommandExecutor> commandExecutor;
        private ApplicationArguments arguments;

        [OneTimeSetUp]
        public void SetupCommandActivationProviderTest()
        {
            commandExecutor = new Mock<ICommandExecutor>();
            provider = new KeyCommandActivationProvider(commandExecutor.Object, new KeyCommandProvider(), new FileCommandProvider());
        }

        [TestFixture]
        public class CreateKeyPair : KeyCommandActivationProviderTest
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
                    EncryptionType = EncryptionType.Pkcs,
                    PrivateKeyPath = "private.pem",
                    PublicKeyPath = "public.pem",
                    ContentType = ContentType.Pem,
                    KeyType = CipherType.Rsa
                };
                
                provider.CreateKeyPair(arguments);
            }

            [Test]
            public void ShouldCreateRsaKeyPair()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateKeyCommand<RsaKey>>(c => c.KeySize == 1024)));
            }

            [Test]
            public void ShouldCreateDsaKeyPair()
            {
                arguments.KeyType = CipherType.Dsa;
                provider.CreateKeyPair(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateKeyCommand<DsaKey>>(c => c.KeySize == 1024)));
            }

            [Test]
            public void ShouldCreateEcKeyPair()
            {
                arguments.KeyType = CipherType.Ec;
                arguments.Curve = "foobar";
                provider.CreateKeyPair(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateKeyCommand<EcKey>>(c => c.Curve == "foobar")));
            }
            
            [TestCase(CipherType.ElGamal)]
            [TestCase(CipherType.Pkcs5Encrypted)]
            [TestCase(CipherType.Pkcs12Encrypted)]
            [TestCase(CipherType.Unknown)]
            public void ShouldThrowExceptionWhenKeyTypeIsNotSupported(CipherType cipherType)
            {
                arguments.KeyType = cipherType;
                Assert.Throws<ArgumentException>(() => { provider.CreateKeyPair(arguments); });
            }
            
            [Test]
            public void ShouldWriteCreatedPrivateKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.First().Out == privateKey && 
                                                                                                                          w.First().FilePath == "private.pem" &&
                                                                                                                          w.First().ContentType == ContentType.Pem &&
                                                                                                                          w.First().EncryptionType == EncryptionType.Pkcs)));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.Last().Out == publicKey && 
                                                                                                                          w.Last().FilePath == "public.pem" &&
                                                                                                                          w.Last().ContentType == ContentType.Pem &&
                                                                                                                          w.Last().EncryptionType == EncryptionType.None)));
            }
        }

        [TestFixture]
        public class VerifyKeyPair : KeyCommandActivationProviderTest
        {
            private IAsymmetricKey publicKey;
            private IAsymmetricKey privateKey;
            
            [SetUp]
            public void Setup()
            {
                arguments = new ApplicationArguments
                {
                    PrivateKeyPath = "private.key",
                    PublicKeyPath = "public.key"
                };
                
                publicKey = Mock.Of<IAsymmetricKey>();
                privateKey = Mock.Of<IAsymmetricKey>();
                
                commandExecutor.Setup(c => c.ExecuteSequence(It.IsAny<IEnumerable<object>>()))
                               .Callback<IEnumerable<object>>(commands =>
                               {
                                   commands.ToList().ForEach(c =>
                                   {
                                       var keyCommand = c as ReadKeyFromFileCommand;
                                       if (keyCommand != null && keyCommand.FilePath == "public.key")
                                       {
                                           keyCommand.Result = publicKey;
                                       }
                                   
                                       if (keyCommand != null && keyCommand.FilePath == "private.key")
                                       {
                                           keyCommand.Result = privateKey;
                                       }                                       
                                   });
                               });
                
                provider.VerifyKeyPair(arguments);
            }
            
            [Test]
            public void ShouldReadPrivateKey()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<ReadKeyFromFileCommand>>(w => w.First().FilePath == "private.key")));
            }

            [Test]
            public void ShouldReadPublicKey()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<ReadKeyFromFileCommand>>(w => w.Last().FilePath == "public.key")));
            }

            [Test]
            public void ShouldVerifyKeyPairWithGivenKeys()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<IVerifyKeyPairCommand>(c => c.PrivateKey == privateKey && c.PublicKey == publicKey)));
            }
        }

        [TestFixture]
        public class ConvertKeyTest : KeyCommandActivationProviderTest
        {
            private IAsymmetricKey publicKey;
            private IAsymmetricKey privateKey;
            
            [SetUp]
            public void SetupConvertKeyTest()
            {
                arguments = new ApplicationArguments
                {
                    ContentType = ContentType.Der
                };

                privateKey = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                publicKey = Mock.Of<IAsymmetricKey>();

                commandExecutor.Setup(ce => ce.Execute(It.IsAny<object>()))
                                .Callback<object>(rc =>
                                {
                                    var readCommand = rc as ReadKeyFromFileCommand;
                                    if (readCommand == null)
                                    {
                                        return;
                                    }
                                    
                                    readCommand.Result = readCommand.IsPrivateKey ? privateKey : publicKey;
                                    readCommand.OriginalContentType = ContentType.Pem;
                                    readCommand.OriginalEncryptionType = readCommand.IsPrivateKey ? EncryptionType.Pkcs : EncryptionType.None;
                                });
            }

            [Test]
            public void ShouldThrowExceptionWhenNoKeysAreSpecified()
            {
                arguments.PrivateKeyPath = string.Empty;
                arguments.PublicKeyPath = string.Empty;
                Assert.Throws<ArgumentException>(() => { provider.ConvertKeyPair(arguments); });
            }

            [Test]
            public void ShouldThrowExceptionWhenNoContentTypeIsSpecified()
            {
                arguments.ContentType = ContentType.NotSpecified;
                arguments.PrivateKeyPath = "foo";
                arguments.PublicKeyPath = "bar";
                
                Assert.Throws<ArgumentException>(() => { provider.ConvertKeyPair(arguments); });
            }
            
            [TestFixture]
            public class ConvertKeyPair : ConvertKeyTest
            {
                [SetUp]
                public void Setup()
                {
                    arguments.PrivateKeyPath = "private.key";
                    arguments.PublicKeyPath = "public.key";
                    arguments.Password = "kensentme";
                    
                    provider.ConvertKeyPair(arguments);
                }
                
                [Test]
                public void ShouldReadPublicKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => !c.IsPrivateKey && c.FilePath == "public.key")));
                }

                [Test]
                public void ShouldReadPrivateKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => c.IsPrivateKey && 
                                                                                               c.FilePath == "private.key" &&
                                                                                               c.Password == "kensentme")));
                }

                [Test]
                public void ShouldWritePublicKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FilePath == "public.key.der" && 
                                                                                                         c.Out == publicKey &&
                                                                                                         c.ContentType == ContentType.Der)));
                }

                [Test]
                public void ShouldWritePrivateKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FilePath == "private.key.der" && 
                                                                                                         c.Out == privateKey &&
                                                                                                         c.ContentType == ContentType.Der &&
                                                                                                         c.EncryptionType == EncryptionType.Pkcs &&
                                                                                                         c.Password == "kensentme")));
                }

                [Test]
                public void ShouldThrowExceptionWhenKeyIsAlreadyInTheGivenType()
                {
                    
                    arguments.ContentType = ContentType.Pem;
                    Assert.Throws<InvalidOperationException>(() => { provider.ConvertKeyPair(arguments); });
                }
            }

            [TestFixture]
            public class ConvertPublicKey : ConvertKeyTest
            {
                [SetUp]
                public void Setup()
                {
                    arguments.PublicKeyPath = "public.key";                   
                    provider.ConvertKeyPair(arguments);
                }
                
                [Test]
                public void ShouldReadPublicKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => !c.IsPrivateKey && c.FilePath == "public.key")));
                }

                [Test]
                public void ShouldNotReadPrivateKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => c.IsPrivateKey)), Times.Never());
                }
                
                [Test]
                public void ShouldWritePublicKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FilePath == "public.key.der" && 
                                                                                                         c.Out == publicKey &&
                                                                                                         c.ContentType == ContentType.Der)));
                }
                
                [Test]
                public void ShouldThrowExceptionWhenKeyIsAlreadyInTheGivenType()
                {
                    arguments.ContentType = ContentType.Pem;
                    Assert.Throws<InvalidOperationException>(() => { provider.ConvertKeyPair(arguments); });
                }
            }
            
            [TestFixture]
            public class ConvertPrivateKey : ConvertKeyTest
            {
                [SetUp]
                public void Setup()
                {
                    arguments.PrivateKeyPath = "private.key";
                    arguments.Password = "kensentme";
                    provider.ConvertKeyPair(arguments);
                }
                
                [Test]
                public void ShouldReadPrivateKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => c.IsPrivateKey && c.FilePath == "private.key")));
                }

                [Test]
                public void ShouldNotReadPublicKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<ReadKeyFromFileCommand>(c => !c.IsPrivateKey)), Times.Never);
                }
                
                [Test]
                public void ShouldWritePrivateKey()
                {
                    commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(c => c.FilePath == "private.key.der" && 
                                                                                                         c.Out == privateKey &&
                                                                                                         c.ContentType == ContentType.Der)));
                }
                
                [Test]
                public void ShouldThrowExceptionWhenKeyIsAlreadyInTheGivenType()
                {
                    arguments.ContentType = ContentType.Pem;
                    Assert.Throws<InvalidOperationException>(() => { provider.ConvertKeyPair(arguments); });
                }
            }
        }
    }
}