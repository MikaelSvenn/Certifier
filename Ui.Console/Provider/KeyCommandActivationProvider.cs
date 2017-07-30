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
            ICreateAsymmetricKeyCommand createKeyCommand = keyCommandProvider.GetCreateKeyCommand(arguments.KeySize);
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
                throw new InvalidOperationException("Key conversion type was not specified.");
            }
            
            ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            commandExecutor.ExecuteSequence(new []{readPrivateKeyFromFile, readPublicKeyFromFile});

            string fileExtension = arguments.ContentType
                                            .ToString()
                                            .ToLower();
            
            if (readPrivateKeyFromFile.OriginalContentType == arguments.ContentType || readPublicKeyFromFile.OriginalContentType == arguments.ContentType)
            {
                throw new InvalidOperationException($"The given key is already in {fileExtension} format.");
            }

            string publicKeyPath = $"{arguments.PublicKeyPath}.{fileExtension}";
            string privateKeyPath = $"{arguments.PrivateKeyPath}.{fileExtension}";
            
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(readPublicKeyFromFile.Result, publicKeyPath, arguments.ContentType);
            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(readPrivateKeyFromFile.Result, privateKeyPath, arguments.ContentType, readPrivateKeyFromFile.OriginalEncryptionType, arguments.Password);
            
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }
    }
}