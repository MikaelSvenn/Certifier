using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class SignatureCommandActivationProvider : ISignatureCommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly SignatureCommandProvider signatureCommandProvider;
        private readonly FileCommandProvider fileCommandProvider;
        private readonly EncodingWrapper encoding;
        private readonly Base64Wrapper base64;

        public SignatureCommandActivationProvider(ICommandExecutor commandExecutor, SignatureCommandProvider signatureCommandProvider, FileCommandProvider fileCommandProvider, EncodingWrapper encoding, Base64Wrapper base64)
        {
            this.commandExecutor = commandExecutor;
            this.signatureCommandProvider = signatureCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
            this.encoding = encoding;
            this.base64 = base64;
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
    }
}