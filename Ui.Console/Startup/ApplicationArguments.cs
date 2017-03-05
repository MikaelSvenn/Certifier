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

        public CreateTarget Create { get; set; }
        public VerifyTarget Verify { get; set; }

        public bool IsValid => !ShowHelp &&
                                 !(Create == CreateTarget.none && Verify == VerifyTarget.none) &&
                                 !(Create != CreateTarget.none && Verify != VerifyTarget.none);
    }
}