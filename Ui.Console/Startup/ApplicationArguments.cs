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
        public string Output { get; set; }
        public string Signature { get; set; }
        public bool ShowHelp { get; set; }
        public KeyEncryptionType EncryptionType { get; set; }
        public CipherType KeyType { get; set; }

        public OperationTarget CreateOperation { get; set; }
        public OperationTarget VerifyOperation { get; set; }

        public bool IsValid => !ShowHelp &&
                                 !(CreateOperation == OperationTarget.None && VerifyOperation == OperationTarget.None) &&
                                 !(CreateOperation != OperationTarget.None && VerifyOperation != OperationTarget.None);

        public bool IsCreate => CreateOperation != OperationTarget.None;
        public bool HasSignature => !string.IsNullOrWhiteSpace(Signature);
        public bool HasOutput => !string.IsNullOrWhiteSpace(Output);
        
    }
}