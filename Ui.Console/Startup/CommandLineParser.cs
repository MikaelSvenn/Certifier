using System.Linq;
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
                .As('k', "keysize")
                .SetDefault(4096);

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

            parser.Setup(argument => argument.Create)
                .As('c', "create");

            parser.Setup(argument => argument.Verify)
                .As('v', "verify");

            var result = parser.Parse(arguments);
            if (result.HasErrors || result.EmptyArgs || result.HelpCalled || result.AdditionalOptionsFound.Any())
            {
                parser.Object.ShowHelp = true;
            }

            return parser.Object;
        }
    }
}