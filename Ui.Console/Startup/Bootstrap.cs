using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using SimpleInjector;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Startup
{
    public class Bootstrap
    {
        public static Container Initialize(string[] applicationArguments)
        {
            var container = new Container();

            container.Register<IConfiguration, PbeConfiguration>();

            container.Register<RsaKeyPairGenerator>();
            container.Register<SecureRandomGenerator>();

            container.Register<IAsymmetricKeyProvider<RsaKey>, RsaKeyProvider>();
            container.Register<IKeyEncryptionProvider, PkcsEncryptionProvider>();
            container.Register<IPkcsFormattingProvider<IAsymmetricKey>, Pkcs8FormattingProvider>();

            container.Register<CommandLineParser>();
            container.Register<ApplicationArguments>(() =>
            {
                var parser = container.GetInstance<CommandLineParser>();
                return parser.ParseArguments(applicationArguments);
            }, Lifestyle.Singleton);

            // Commands
            container.Register(typeof(ICommandHandler<>), new[] { typeof(ICommandHandler<>).Assembly });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EncryptionPasswordValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RsaKeySizeValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyEncryptionDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>), typeof(Pkcs8FormattingDecorator));

            container.Verify();

            return container;
        }
    }
}