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
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<RsaKey>(arguments.KeySize);
                        break;
                    case CipherType.Dsa:
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<DsaKey>(arguments.KeySize);
                        break;
                    case CipherType.Ec:
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<EcKey>(arguments.Curve);
                        break;
                    case CipherType.ElGamal:
                        createKeyCommand = keyCommandProvider.GetCreateKeyCommand<ElGamalKey>(arguments.KeySize, arguments.UseRfc3526Prime);
                        break;
                    default:
                        throw new ArgumentException("Key type not supported.");
            }
            
            commandExecutor.Execute(createKeyCommand);
            
            if (createKeyCommand.Curve == "curve25519" && arguments.ContentType == ContentType.OpenSsh)
            {
                WriteFileCommand<IAsymmetricKeyPair> writeOpenSshCurve25519PrivateKey = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKeyPair>(createKeyCommand.Result, arguments.PrivateKeyPath, arguments.ContentType, arguments.EncryptionType, arguments.Password);
                WriteFileCommand<IAsymmetricKey> writeOpenSshCurve25519PublicKey = fileCommandProvider.GetWriteKeyToFileCommand(createKeyCommand.Result.PrivateKey, arguments.PublicKeyPath, arguments.ContentType);
                commandExecutor.Execute(writeOpenSshCurve25519PrivateKey);
                commandExecutor.Execute(writeOpenSshCurve25519PublicKey);
                return;
            }
            
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteKeyToFileCommand(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath, arguments.ContentType);
            commandExecutor.Execute(writePublicKeyToFile);

            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteKeyToFileCommand(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath, arguments.ContentType, arguments.EncryptionType, arguments.Password);
            commandExecutor.Execute(writePrivateKeyToFile);
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

            if (arguments.IsContentTypeSsh && readKeyFromFile.Result.IsPrivateKey)
            {
                throw new InvalidOperationException("Private key cannot be converted to SSH format.");
            }
            
            string keyPath = $"{readKeyFromFile.FilePath}.{fileExtension}";
            WriteFileCommand<IAsymmetricKey> writeKeyToFile = fileCommandProvider.GetWriteKeyToFileCommand(readKeyFromFile.Result, keyPath, arguments.ContentType, readKeyFromFile.OriginalEncryptionType, arguments.Password);
            commandExecutor.Execute(writeKeyToFile);
        }
    }
}