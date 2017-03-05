using Core.Configuration;
using Core.Interfaces;
using Core.Services;
using Crypto.Generators;
using Crypto.Providers;
using SimpleInjector;

namespace Ui.Console.Startup
{
    public class Bootstrap
    {
        public static Container Initialize(string[] applicationArguments)
        {
            var container = new Container();

            container.Register<IConfiguration, KeyProviderConfiguration>();

            container.Register<RsaKeyPairGenerator>();
            container.Register<SecureRandomGenerator>();
            container.Register<SignatureAlgorithmGenerator>();

            container.Register<IKeyProvider, RsaKeyPairProvider>();
            container.Register<ISignatureProvider, SignatureProvider>();

            container.Register<IKeyService, KeyService>();
            container.Register<ISignatureService, SignatureService>();

            container.Register<CommandLineParser>();
            container.Register<ApplicationArguments>(() =>
            {
                var parser = container.GetInstance<CommandLineParser>();
                return parser.ParseArguments(applicationArguments);
            }, Lifestyle.Singleton);

            container.Verify();

            return container;
        }
    }
}