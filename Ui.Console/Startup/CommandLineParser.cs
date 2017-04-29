using System.Linq;
using Core.Model;
using Fclp;

namespace Ui.Console.Startup
{
    public class CommandLineParser
    {
        public ApplicationArguments ParseArguments(string[] arguments)
        {
            var parser = new FluentCommandLineParser<ApplicationArguments>();

            parser.SetupHelp("?", "h", "help");

            parser.Setup(argument => argument.KeySize)
                .As('b', "keysize")
                .SetDefault(4096);

            parser.Setup(argument => argument.KeyType)
                .As('k', "keytype")
                .SetDefault(CipherType.Rsa);

            parser.Setup(argument => argument.EncryptionType)
                .As('e', "encryption")
                .SetDefault(KeyEncryptionType.None);

            parser.Setup(argument => argument.Password)
                .As('p', "password")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.PrivateKeyPath)
                .As("privatekey")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.PublicKeyPath)
                .As("publickey")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.DataPath)
                .As('f', "file")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.Signature)
                .As('s', "signature")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.CreateOperation)
                .As('c', "create")
                .SetDefault(OperationTarget.none);

            parser.Setup(argument => argument.VerifyOperation)
                .As('v', "verify")
                .SetDefault(OperationTarget.none);

            var result = parser.Parse(arguments);
            if (result.HasErrors || result.EmptyArgs || result.HelpCalled || result.AdditionalOptionsFound.Any())
            {
                parser.Object.ShowHelp = true;
            }

            return parser.Object;
        }
    }
}