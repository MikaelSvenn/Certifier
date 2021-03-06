﻿using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class CreateDsaKeyCommandHandlerTest
    {
        private CreateDsaKeyCommandHandler commandHandler;
        private Mock<IKeyProvider<DsaKey>> keyProvider;
        private IAsymmetricKeyPair keyPair;
        private CreateKeyCommand<DsaKey> command;
        
        [SetUp]
        public void Setup()
        {
            keyPair = new AsymmetricKeyPair(null, null);    
            keyProvider = new Mock<IKeyProvider<DsaKey>>();
            keyProvider.Setup(k => k.CreateKeyPair(7))
                       .Returns(keyPair);

            command = new CreateKeyCommand<DsaKey>
            {
                KeySize = 7
            };
            
            commandHandler = new CreateDsaKeyCommandHandler(keyProvider.Object);
            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldCreateKey()
        {
            keyProvider.Verify(kp => kp.CreateKeyPair(7));
        }

        [Test]
        public void ShouldSetCreatedKeyAsCommandResult()
        {
            Assert.AreEqual(keyPair, command.Result);
        }
    }
}