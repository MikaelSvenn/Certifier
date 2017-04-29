using Core.Model;

namespace Ui.Console.Startup
{
    public class ApplicationArguments
    {
        public int KeySize { get; set; }
        public string Password { get; set; }
        public string PrivateKeyPath { get; set; }
        public string PublicKeyPath { get; set; }
        public string DataPath { get; set; }
        public string Signature { get; set; }
        public bool ShowHelp { get; set; }
        public KeyEncryptionType EncryptionType { get; set; }
        public CipherType KeyType { get; set; }

        public OperationTarget CreateOperation { get; set; }
        public OperationTarget VerifyOperation { get; set; }

        public bool IsValid => !ShowHelp &&
                                 !(CreateOperation == OperationTarget.none && VerifyOperation == OperationTarget.none) &&
                                 !(CreateOperation != OperationTarget.none && VerifyOperation != OperationTarget.none);

        public bool IsCreate => CreateOperation != OperationTarget.none;
    }
}