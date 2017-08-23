using Core.Interfaces;
using Core.Model;

namespace Ui.Console.Command
{
    public interface ICreateAsymmetricKeyCommand : ICommandWithResult<IAsymmetricKeyPair>
    {
        int KeySize { get; set; }
        string Curve { get; set; }
    }
}