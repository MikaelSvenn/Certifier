using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public class CreateRsaKeyCommand : ICreateAsymmetricKeyCommand
    {
        public IAsymmetricKeyPair Result { get; set; }
        public int KeySize { get; set; }
        public string Password { get; set; }
        public KeyEncryptionType EncryptionType { get; set; }
    }
}