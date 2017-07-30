using System;
using System.Collections.Generic;
using Core.Configuration;
using Core.Interfaces;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Provider;

namespace Ui.Console.Test.Integration
{
    [TestFixture]
    public class ConvertKeyPairTest
    {
        private Dictionary<string, byte[]> files;
        private Mock<FileWrapper> file;
        private AsymmetricKeyProvider asymmetricKeyProvider;
        private PkcsEncryptionProvider encryptionProvider;
        private Pkcs8FormattingProvider pkcs8FormattingProvider;
        private EncodingWrapper encodingWrapper;

        [SetUp]
        public void SetupConvertKeyPairTest()
        {
            files = new Dictionary<string, byte[]>();

            file = new Mock<FileWrapper>();
            file.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((path, content) =>
                {
                    files.Add(path, content);
                });
            
            file.Setup(f => f.ReadAllBytes(It.IsAny<string>()))
                .Returns<string>(givenFile => files[givenFile]);
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
        }

        public void PopulateRsaKeys()
        {
            var rsaKeyPairGenerator = new RsaKeyPairGenerator(new SecureRandomGenerator());
            var rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), rsaKeyProvider, new KeyInfoWrapper());
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            
            IAsymmetricKeyPair rsaKeyPair = rsaKeyProvider.CreateKeyPair(1024);
            
            files.Add("private.rsa.der", rsaKeyPair.PrivateKey.Content);
            files.Add("public.rsa.der", rsaKeyPair.PublicKey.Content);
            files.Add("private.rsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(rsaKeyPair.PrivateKey)));
            files.Add("public.rsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(rsaKeyPair.PublicKey)));
            files.Add("private.rsa.encrypted.der", encryptionProvider.EncryptPrivateKey(rsaKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.rsa.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(rsaKeyPair.PrivateKey, "foobarbaz"))));
        }
        
        [TearDown]
        public void TeardownConvertKeyPairTest()
        {
            ContainerProvider.ClearContainer();
        }

        [TestFixture]
        public class ErrorCases : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }
            
            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.pem", "-t", "der"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "der"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateWhenKeyAlreadyIsInGivenFormat()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => { Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem", "-t", "pem"}); });
                Assert.AreEqual("The given key is already in pem format.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingKeyTypeParameter()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => { Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem"}); });
                Assert.AreEqual("Key conversion type was not specified.", exception.Message);
            }
        }

        [TestFixture]
        public class ConvertUnencryptedRsaKeyPair : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.rsa.der"], files["private.rsa.pem.der"]);
                CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.pem.der"]);
            }
            
            [Test]
            public void ShouldConvertDerToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "--publickey", "public.rsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.rsa.pem"], files["private.rsa.der.pem"]);
                CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.der.pem"]);
            }
        }

        [TestFixture]
        public class ConvertEncryptedRsaKeyPair : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.encrypted.pem", "-p", "foobarbaz", "--publickey", "public.rsa.pem", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.rsa.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.rsa.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertDerToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.encrypted.der", "-p", "foobarbaz", "--publickey", "public.rsa.der", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.rsa.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.rsa.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }
    }
}