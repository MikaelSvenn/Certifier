using Core.Interfaces;

namespace Ui.Console.Command
{
    public class CreateKeyCommand<T> : ICreateAsymmetricKeyCommand where T : IAsymmetricKey
    {
        public IAsymmetricKeyPair Result { get; set; }
        public int KeySize { get; set; }
        public string Curve { get; set; }
    }
}