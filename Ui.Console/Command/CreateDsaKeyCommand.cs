using Core.Interfaces;

namespace Ui.Console.Command
{
    public class CreateDsaKeyCommand : ICreateAsymmetricKeyCommand
    {
        public IAsymmetricKeyPair Result { get; set; }
        public int KeySize { get; set; }
    }
}