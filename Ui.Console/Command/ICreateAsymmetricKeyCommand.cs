using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public interface ICreateAsymmetricKeyCommand : ICommandWithResult<IAsymmetricKeyPair>
    {
        int KeySize { get; set; }
        string Password { get; set; }
        KeyEncryptionType EncryptionType { get; set; }
    }
}