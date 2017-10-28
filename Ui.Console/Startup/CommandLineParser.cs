using System.Linq;
using Core.Model;
using Fclp;
using Fclp.Internals.Extensions;

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
                .SetDefault(EncryptionType.None);

            parser.Setup(argument => argument.Password)
                .As('p', "password")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.PrivateKeyPath)
                .As("privatekey")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.PublicKeyPath)
                .As("publickey")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.Input)
                .As('i', "in")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.FileInput)
                .As('f', "file")
                .SetDefault(string.Empty);
            
            parser.Setup(argument => argument.FileOutput)
                .As('o', "out")
                .SetDefault(string.Empty);
            
            parser.Setup(argument => argument.Signature)
                .As('s', "signature")
                .SetDefault(string.Empty);

            parser.Setup(argument => argument.CreateOperation)
                .As('c', "create")
                .SetDefault(OperationTarget.None);

            parser.Setup(argument => argument.VerifyOperation)
                .As('v', "verify")
                .SetDefault(OperationTarget.None);

            parser.Setup(argument => argument.ContentType)
                .As('t', "type")
                .SetDefault(ContentType.NotSpecified);

            parser.Setup(argument => argument.IsConvertOperation)
                  .As("convert")
                  .SetDefault(false);

            parser.Setup(argument => argument.Curve)
                  .As("curve")
                  .SetDefault("curve25519");

            parser.Setup(argument => argument.UseRfc3526Prime)
                  .As("fast")
                  .SetDefault(false);

            var result = parser.Parse(arguments);
            if (result.HasErrors || result.EmptyArgs || result.HelpCalled || result.AdditionalOptionsFound.Any())
            {
                parser.Object.ShowHelp = true;
            }

            return parser.Object;
        }
    }
}