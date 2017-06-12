using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class CommandActivationProvider : ICommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly KeyCommandProvider keyCommandProvider;
        private readonly FileCommandProvider fileCommandProvider;
        private readonly SignatureCommandProvider signatureCommandProvider;
        private readonly EncodingWrapper encoding;
        private readonly Base64Wrapper base64;

        public CommandActivationProvider(ICommandExecutor commandExecutor, KeyCommandProvider keyCommandProvider, FileCommandProvider fileCommandProvider, SignatureCommandProvider signatureCommandProvider, EncodingWrapper encoding, Base64Wrapper base64)
        {
            this.commandExecutor = commandExecutor;
            this.keyCommandProvider = keyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
            this.signatureCommandProvider = signatureCommandProvider;
            this.encoding = encoding;
            this.base64 = base64;
        }

        public void CreateKeyPair(ApplicationArguments arguments)
        {
            ICreateAsymmetricKeyCommand createKeyCommand = keyCommandProvider.GetCreateKeyCommand(arguments.KeySize, arguments.EncryptionType, arguments.Password);
            commandExecutor.Execute(createKeyCommand);

            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }

        public void CreateSignature(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            commandExecutor.Execute(readPrivateKeyFromFile);

            byte[] contentToSign;
            if (arguments.HasFileInput)
            {
                ReadFileCommand<byte[]> readFileToSign = fileCommandProvider.GetReadFileCommand<byte[]>(arguments.FileInput);
                commandExecutor.Execute(readFileToSign);  
                contentToSign = readFileToSign.Result;
            }
            else
            {
                contentToSign = encoding.GetBytes(arguments.Input);
            }

            CreateSignatureCommand createSignature = signatureCommandProvider.GetCreateSignatureCommand(readPrivateKeyFromFile.Result, contentToSign);
            commandExecutor.Execute(createSignature);

            if (arguments.HasFileOutput)
            {
                WriteFileCommand<Signature> writeSignatureTofile = fileCommandProvider.GetWriteToFileCommand(createSignature.Result, arguments.FileOutput);
                commandExecutor.Execute(writeSignatureTofile);
                return;
            }

            WriteToStdOutCommand<Signature> writeSignatureToStdOut = fileCommandProvider.GetWriteToStdOutCommand<Signature>(createSignature.Result);
            commandExecutor.Execute(writeSignatureToStdOut);
        }

        public void VerifySignature(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
            commandExecutor.Execute(readPublicKeyFromFile);

            byte[] contentToVerify;
            if (arguments.HasFileInput)
            {
                ReadFileCommand<byte[]> readFileToVerify = fileCommandProvider.GetReadFileCommand<byte[]>(arguments.FileInput);
                commandExecutor.Execute(readFileToVerify);
                contentToVerify = readFileToVerify.Result;
            }
            else
            {
                contentToVerify = encoding.GetBytes(arguments.Input);
            }

            byte[] signatureToVerify;
            if (base64.IsBase64(arguments.Signature))
            {
                signatureToVerify = base64.FromBase64String(arguments.Signature);
            }
            else
            {
                ReadFileCommand<byte[]> readSignatureToVerify = fileCommandProvider.GetReadFileCommand<byte[]>(arguments.Signature);
                commandExecutor.Execute(readSignatureToVerify);
                
                string base64Signature = encoding.GetString(readSignatureToVerify.Result);
                signatureToVerify = base64.FromBase64String(base64Signature);
            }
            
            VerifySignatureCommand verifySignature = signatureCommandProvider.GetVerifySignatureCommand(readPublicKeyFromFile.Result, contentToVerify, signatureToVerify);
            commandExecutor.Execute(verifySignature);
        }

        public void VerifyKeyPair(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            commandExecutor.ExecuteSequence(new []{readPrivateKeyFromFile, readPublicKeyFromFile});

            IVerifyKeyPairCommand verifyKeyPairCommand = keyCommandProvider.GetVerifyKeyPairCommand(readPublicKeyFromFile.Result, readPrivateKeyFromFile.Result);
            commandExecutor.Execute(verifyKeyPairCommand);
        }
    }
}