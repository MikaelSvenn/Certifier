using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
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
                    EncryptionType = KeyEncryptionType.None,
                    PrivateKeyPath = "private.pem",
                    PublicKeyPath = "public.pem",
                    ContentType = ContentType.Pem
                };

                provider.CreateKeyPair(arguments);
            }

            [Test]
            public void ShouldCreateRsaKeyPair()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateRsaKeyCommand>(c => c.EncryptionType == KeyEncryptionType.None && 
                                                                                        c.KeySize == 1024)));
            }

            [Test]
            public void ShouldWriteCreatedPrivateKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.First().Out == privateKey && 
                                                                                                                          w.First().FilePath == "private.pem" &&
                                                                                                                          w.First().ContentType == ContentType.Pem)));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.Last().Out == publicKey && 
                                                                                                                          w.Last().FilePath == "public.pem" &&
                                                                                                                          w.Last().ContentType == ContentType.Pem)));
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
                                   commands.ForEach(c =>
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
    }
}