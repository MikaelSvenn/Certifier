using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using SimpleInjector;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;
using Ui.Console.Provider;

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

            container.Register<ICommandActivationProvider, CommandActivationProvider>();
            container.Register<ICommandExecutor, CommandExecutor>();

            // Commands
            container.Register(typeof(ICommandHandler<>), new[] { typeof(ICommandHandler<>).Assembly });
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EncryptionPasswordValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RsaKeySizeValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyEncryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyDecryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(KeyFilePathValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8WriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8ReadFormattingDecorator<>));

            container.Verify();

            return container;
        }
    }
}