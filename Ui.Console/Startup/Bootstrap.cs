using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using SimpleInjector;

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
            container.Register<SignatureAlgorithmIdentifierMapper>();

            container.Register<IAsymmetricKeyProvider<RsaKey>, RsaKeyProvider>();
            container.Register<ISignatureProvider, SignatureProvider>();

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