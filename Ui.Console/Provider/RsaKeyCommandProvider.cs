using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class RsaKeyCommandProvider
    {
        public CreateRsaKeyCommand GetCreateRsaKeyCommand(ApplicationArguments arguments)
        {
            return new CreateRsaKeyCommand
            {
                KeySize = arguments.KeySize,
                EncryptionType = arguments.EncryptionType,
                Password = arguments.Password
            };
        }
    }
}