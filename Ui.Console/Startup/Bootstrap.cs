using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using SimpleInjector;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;
using Ui.Console.Provider;

namespace Ui.Console.Startup
{
    public class Bootstrap
    {
        public static Container Initialize(string[] applicationArguments)
        {
            Container container = ContainerProvider.GetContainer();

            container.Register<IConfiguration, PbeConfiguration>();

            container.Register<RsaKeyPairGenerator>();
            container.Register<SecureRandomGenerator>();

            container.Register<IKeyProvider<RsaKey>, RsaKeyProvider>();
            container.Register<IAsymmetricKeyProvider, AsymmetricKeyProvider>();
            container.Register<IKeyEncryptionProvider, PkcsEncryptionProvider>();
            container.Register<IPkcsFormattingProvider<IAsymmetricKey>, Pkcs8FormattingProvider>();
            container.Register<ISignatureProvider, SignatureProvider>();
            
            container.Register<CommandLineParser>();
            container.Register<ApplicationArguments>(() =>
            {
                var parser = container.GetInstance<CommandLineParser>();
                return parser.ParseArguments(applicationArguments);
            }, Lifestyle.Singleton);

            container.Register<IKeyCommandActivationProvider, KeyCommandActivationProvider>();
            container.Register<ISignatureCommandActivationProvider, SignatureCommandActivationProvider>();
            container.Register<ICommandExecutor, CommandExecutor>();

            // Commands
            container.Register(typeof(ICommandHandler<>), new[] { typeof(ICommandHandler<>).Assembly });
            container.Register<ICommandHandler<WriteFileCommand<IAsymmetricKey>>, WriteToFileCommandHandler<IAsymmetricKey>>();
            container.Register<ICommandHandler<WriteFileCommand<Signature>>, WriteToFileCommandHandler<Signature>>();
            container.Register<ICommandHandler<WriteToStdOutCommand<Signature>>, WriteToStdOutCommandHandler<Signature>>();
            container.Register<ICommandHandler<ReadFileCommand<byte[]>>, ReadFileCommandHandler<byte[]>>();
            
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EncryptionPasswordValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RsaKeySizeValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(FilePathValidationDecorator<,>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteKeyToFilePathValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ReadKeyFromFilePathValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyEncryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8ReadFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ReadKeyConversionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyDecryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8WriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteToFileBase64FormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteToStdOutBase64FormattingDecorator<>));

            container.Verify();

            return container;
        }
    }
}