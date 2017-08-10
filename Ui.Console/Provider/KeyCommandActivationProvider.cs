using System;
using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class KeyCommandActivationProvider : IKeyCommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly KeyCommandProvider keyCommandProvider;
        private readonly FileCommandProvider fileCommandProvider;

        public KeyCommandActivationProvider(ICommandExecutor commandExecutor, KeyCommandProvider keyCommandProvider, FileCommandProvider fileCommandProvider)
        {
            this.commandExecutor = commandExecutor;
            this.keyCommandProvider = keyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
        }

        public void CreateKeyPair(ApplicationArguments arguments)
        {
            ICreateAsymmetricKeyCommand createKeyCommand;
            switch (arguments.KeyType)
            {
                    case CipherType.Rsa:
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<CreateRsaKeyCommand>(arguments.KeySize);
                        break;
                    case CipherType.Dsa:
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<CreateDsaKeyCommand>(arguments.KeySize);
                        break;
                    default:
                        throw new ArgumentException("Key type not supported.");
            }
            
            commandExecutor.Execute(createKeyCommand);
            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath, arguments.ContentType, arguments.EncryptionType, arguments.Password);
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath, arguments.ContentType);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }
        
        public void VerifyKeyPair(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            commandExecutor.ExecuteSequence(new []{readPrivateKeyFromFile, readPublicKeyFromFile});

            IVerifyKeyPairCommand verifyKeyPairCommand = keyCommandProvider.GetVerifyKeyPairCommand(readPublicKeyFromFile.Result, readPrivateKeyFromFile.Result);
            commandExecutor.Execute(verifyKeyPairCommand);
        }

        public void ConvertKeyPair(ApplicationArguments arguments)
        {
            if (arguments.ContentType == ContentType.NotSpecified)
            {
                throw new ArgumentException("Target type for key conversion was not specified.");
            }

            if (!arguments.HasPrivateKey && !arguments.HasPublicKey)
            {
                throw new ArgumentException("No keys were specified for conversion.");
            }
            
            if (arguments.HasPublicKey)
            {
                ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
                ConvertKey(readPublicKeyFromFile, arguments);
            }

            if (!arguments.HasPrivateKey)
            {
                return;
            }
            
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            ConvertKey(readPrivateKeyFromFile, arguments);
        }

        private void ConvertKey(ReadKeyFromFileCommand readKeyFromFile, ApplicationArguments arguments)
        {
            commandExecutor.Execute(readKeyFromFile);
            string fileExtension = arguments.ContentType
                                   .ToString()
                                   .ToLower();
            
            if (readKeyFromFile.OriginalContentType == arguments.ContentType)
            {
                throw new InvalidOperationException($"The given key {readKeyFromFile.FilePath} is already in {fileExtension} format.");
            }

            string keyPath = $"{readKeyFromFile.FilePath}.{fileExtension}";
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(readKeyFromFile.Result, keyPath, arguments.ContentType, readKeyFromFile.OriginalEncryptionType, arguments.Password);
            commandExecutor.Execute(writePublicKeyToFile);
        }
    }
}