using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console;
using Ui.Console.Provider;

namespace Integration.VerifyKey.Test
{
    [TestFixture]
    public class VerifyKeyPairTest
    {
        private Dictionary<string, byte[]> files;
        private Mock<FileWrapper> file;
        private Mock<ConsoleWrapper> console;
        private RsaKeyProvider rsaKeyProvider;
        private DsaKeyProvider dsaKeyProvider;
        private EcKeyProvider ecKeyProvider;
        private EncodingWrapper encoding;
        private Pkcs8FormattingProvider pkcs8Formatter;
        
        [SetUp]
        public void SetupVerifyKeyPairTest()
        {
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            
            rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
            dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
            ecKeyProvider = new EcKeyProvider(asymmetricKeyPairGenerator);
            
            encoding = new EncodingWrapper();
            pkcs8Formatter = new Pkcs8FormattingProvider(new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, dsaKeyProvider, ecKeyProvider));
            
            files = new Dictionary<string, byte[]>();
            file = new Mock<FileWrapper>();            
            file.Setup(f => f.ReadAllBytes(It.IsAny<string>()))
                .Returns<string>(givenFile => files[givenFile]);
            
            console = new Mock<ConsoleWrapper>();
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
            container.Register<ConsoleWrapper>(() => console.Object);
        }
        
        private void PopulateRsaKeys()
        {
            IAsymmetricKeyPair rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);
            string firstPublicKey = pkcs8Formatter.GetAsPem(rsaKeyPair.PublicKey);
            string firstPrivateKey = pkcs8Formatter.GetAsPem(rsaKeyPair.PrivateKey);

            files.Add("private.rsa.first", encoding.GetBytes(firstPrivateKey));
            files.Add("public.rsa.first", encoding.GetBytes(firstPublicKey));

            rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);

            string secondPublicKey = pkcs8Formatter.GetAsPem(rsaKeyPair.PublicKey);
            string secondPrivateKey = pkcs8Formatter.GetAsPem(rsaKeyPair.PrivateKey);
            files.Add("private.rsa.second", encoding.GetBytes(secondPrivateKey));
            files.Add("public.rsa.second", encoding.GetBytes(secondPublicKey));
        }

        private void PopulateDsaKeys()
        {
            IAsymmetricKeyPair dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
            string firstPublicKey = pkcs8Formatter.GetAsPem(dsaKeyPair.PublicKey);
            string firstPrivateKey = pkcs8Formatter.GetAsPem(dsaKeyPair.PrivateKey);

            files.Add("private.dsa.first", encoding.GetBytes(firstPrivateKey));
            files.Add("public.dsa.first", encoding.GetBytes(firstPublicKey));

            dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);

            string secondPublicKey = pkcs8Formatter.GetAsPem(dsaKeyPair.PublicKey);
            string secondPrivateKey = pkcs8Formatter.GetAsPem(dsaKeyPair.PrivateKey);
            files.Add("private.dsa.second", encoding.GetBytes(secondPrivateKey));
            files.Add("public.dsa.second", encoding.GetBytes(secondPublicKey));
        }

        private void PopulateEcKeys()
        {
            IAsymmetricKeyPair ecKeyPair = ecKeyProvider.CreateKeyPair("sect283r1");
            string firstPublicKey = pkcs8Formatter.GetAsPem(ecKeyPair.PublicKey);
            string firstPrivateKey = pkcs8Formatter.GetAsPem(ecKeyPair.PrivateKey);

            files.Add("private.ec.first", encoding.GetBytes(firstPrivateKey));
            files.Add("public.ec.first", encoding.GetBytes(firstPublicKey));

            ecKeyPair = ecKeyProvider.CreateKeyPair("sect283r1");

            string secondPublicKey = pkcs8Formatter.GetAsPem(ecKeyPair.PublicKey);
            string secondPrivateKey = pkcs8Formatter.GetAsPem(ecKeyPair.PrivateKey);
            files.Add("private.ec.second", encoding.GetBytes(secondPrivateKey));
            files.Add("public.ec.second", encoding.GetBytes(secondPublicKey));
        }
        
        [TearDown]
        public void TeardownVerifyKeyPairTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class VerifyRsaKeyPair : VerifyKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }

            [Test]
            public void ShouldNotThrowExceptionWhenKeyPairIsValid()
            {
                Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.rsa.first", "--privatekey", "private.rsa.first"}));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyPairIsNotValid()
            {
                Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.rsa.first", "--privatekey", "private.rsa.second"}));
            }
        }

        [TestFixture]
        public class VerifyDsaKeyPair : VerifyKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateDsaKeys();
            }

            [Test]
            public void ShouldNotThrowExceptionWhenKeyPairIsValid()
            {
                Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.dsa.first", "--privatekey", "private.dsa.first"}));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyPairIsNotValid()
            {
                Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.dsa.first", "--privatekey", "private.dsa.second"}));
            }
        }

        [TestFixture]
        public class VerifyEcKeyPair : VerifyKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateEcKeys();
            }
            
            [Test]
            public void ShouldNotThrowExceptionWhenKeyPairIsValid()
            {
                Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.ec.first", "--privatekey", "private.ec.first"}));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyPairIsNotValid()
            {
                Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.ec.first", "--privatekey", "private.ec.second"}));
            }
        }
        
        [TestFixture]
        public class WhenKeysAreNotTheSameType : VerifyKeyPairTest
        {
            [Test]
            public void ShouldThrowException()
            {
                IAsymmetricKeyPair rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);
                string publicKey = pkcs8Formatter.GetAsPem(rsaKeyPair.PublicKey);

                files.Add("public.rsa", encoding.GetBytes(publicKey));
            
                IAsymmetricKeyPair dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
                string privateKey = pkcs8Formatter.GetAsPem(dsaKeyPair.PrivateKey);

                files.Add("private.dsa", encoding.GetBytes(privateKey));
            
                Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.rsa", "--privatekey", "private.dsa"}));
            }
        }
    }
}