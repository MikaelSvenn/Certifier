﻿using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Formatters;
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
            container.Register<AsymmetricKeyPairGenerator>();
            container.Register<SecureRandomGenerator>();

            container.Register<IKeyProvider<RsaKey>, RsaKeyProvider>();
            container.Register<IRsaKeyProvider, RsaKeyProvider>();
            container.Register<IKeyProvider<DsaKey>, DsaKeyProvider>();
            container.Register<IDsaKeyProvider, DsaKeyProvider>();
            container.Register<IElGamalKeyProvider, ElGamalKeyProvider>();
            container.Register<IEcKeyProvider, EcKeyProvider>();
            container.Register<IAsymmetricKeyProvider, AsymmetricKeyProvider>();
            container.Register<IKeyEncryptionProvider, KeyEncryptionProvider>();
            container.Register<IPemFormattingProvider<IAsymmetricKey>, Pkcs8PemFormattingProvider>();
            container.Register<IPemFormattingProvider<IEcKey>, EcPemFormattingProvider>();
            container.Register<ISignatureProvider, SignatureProvider>();
            container.Register<Ssh2ContentFormatter>();
            container.Register<ISshFormattingProvider, SshFormattingProvider>();
            container.Register<ISshKeyProvider, SshKeyProvider>();
            
            container.Register<CommandLineParser>();
            container.Register<ApplicationArguments>(() =>
            {
                var parser = container.GetInstance<CommandLineParser>();
                return parser.ParseArguments(applicationArguments);
            }, Lifestyle.Singleton);

            container.Register<IKeyCommandActivationProvider, KeyCommandActivationProvider>();
            container.Register<ISignatureCommandActivationProvider, SignatureCommandActivationProvider>();
            container.Register<ICommandExecutor, CommandExecutor>();
            
            container.Register<Help>();

            // Command handler
            container.Register(typeof(ICommandHandler<>), new[] { typeof(ICommandHandler<>).Assembly });
            container.Register<ICommandHandler<WriteFileCommand<IAsymmetricKey>>, WriteToFileCommandHandler<IAsymmetricKey>>();
            container.Register<ICommandHandler<WriteFileCommand<IAsymmetricKeyPair>>, WriteToFileCommandHandler<IAsymmetricKeyPair>>();
            container.Register<ICommandHandler<WriteFileCommand<Signature>>, WriteToFileCommandHandler<Signature>>();
            container.Register<ICommandHandler<WriteToStdOutCommand<Signature>>, WriteToStdOutCommandHandler<Signature>>();
            container.Register<ICommandHandler<ReadFileCommand<byte[]>>, ReadFileCommandHandler<byte[]>>();
            
            //Create key
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RsaKeySizeValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(DsaKeySizeValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ElGamalKeySizeValidationDecorator<>));

            //Read file
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(FilePathValidationDecorator<,>));
            
            //Read key
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8PemReadFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EcSec1PemReadFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SshReadFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8DerReadFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyDecryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AesKeyDecryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ReadKeyFromFilePathValidationDecorator<>));

            //Write content
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteToFileBase64FormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteToStdOutBase64FormattingDecorator<>));
            
            //Write key
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Ssh2WriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(OpenSshPublicKeyWriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(OpenSshPrivateKeyWriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8PemWriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Pkcs8DerWriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EcSec1PemWriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(PkcsKeyEncryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AesKeyEncryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(OpenSshKeyEncryptionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EncryptionPasswordValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EcSec1WriteFormattingDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EcSec1EncryptionValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(EcSec1ValidationDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(WriteKeyToFilePathValidationDecorator<>));

            //Verify key
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(VerifyKeyTypeValidationDecorator<>));

            container.Verify();

            return container;
        }
    }
}