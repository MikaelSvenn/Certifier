using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Startup;

namespace Ui.Console
{
    internal class Certifier
    {
        public static void Main(string[] commandLineArguments)
        {
            var container = Bootstrap.Initialize(commandLineArguments);
            var arguments = container.GetInstance<ApplicationArguments>();

            if (arguments.ShowHelp || !arguments.IsValid)
            {
                ShowHelp();
                return;
            }

            var createKey = container.GetInstance<ICommandHandler<CreateRsaKeyCommand>>();
            var writeToFile = container.GetInstance<ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>>();

            var createKeyPair = new CreateRsaKeyCommand
            {
                EncryptionType = KeyEncryptionType.Pkcs,
                KeySize = 4096,
                Password = "foobar"
            };

            createKey.Excecute(createKeyPair);

            var writePrivateKey = new WriteToTextFileCommand<IAsymmetricKey>
            {
                Content = createKeyPair.Result.PrivateKey,
                Destination = "privatekey.pem"
            };
            var writePublicKey = new WriteToTextFileCommand<IAsymmetricKey>
            {
                Content = createKeyPair.Result.PublicKey,
                Destination = "publickey.pem"
            };

            writeToFile.Excecute(writePrivateKey);
            writeToFile.Excecute(writePublicKey);

            System.Console.WriteLine("Done!");
        }

        public static void ShowHelp()
        {
            System.Console.WriteLine("HELP!");
        }
    }
}