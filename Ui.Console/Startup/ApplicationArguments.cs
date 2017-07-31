using Core.Model;

namespace Ui.Console.Startup
{
    public class ApplicationArguments
    {
        public int KeySize { get; set; }
        public string Password { get; set; }
        public string PrivateKeyPath { get; set; }
        public string PublicKeyPath { get; set; }
        public string Input { get; set; }
        public string FileInput { get; set; }
        public string FileOutput { get; set; }
        public string Signature { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public CipherType KeyType { get; set; }

        public OperationTarget CreateOperation { get; set; }
        public OperationTarget VerifyOperation { get; set; }

        public bool ShowHelp { get; set; }
        public bool IsValidOperation => !(CreateOperation == OperationTarget.None && VerifyOperation == OperationTarget.None) &&
                                        !(CreateOperation != OperationTarget.None && VerifyOperation != OperationTarget.None) ||
                                        IsConvertOperation;
        
        public bool IsCreate => CreateOperation != OperationTarget.None;
        public bool HasSignature => !string.IsNullOrWhiteSpace(Signature);
        public bool HasFileOutput => !string.IsNullOrWhiteSpace(FileOutput);
        public bool HasFileInput => !string.IsNullOrWhiteSpace(FileInput);
        public ContentType ContentType { get; set; }
        public bool IsConvertOperation { get; set; }
        public bool HasPublicKey => !string.IsNullOrWhiteSpace(PublicKeyPath);
        public bool HasPrivateKey => !string.IsNullOrWhiteSpace(PrivateKeyPath);
    }
}