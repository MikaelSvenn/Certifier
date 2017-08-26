﻿using System;
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
using Ui.Console;
using Ui.Console.Provider;

namespace Integration.ConvertKey.Test
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
            var rsaKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var rsaKeyProvider = new RsaKeyProvider(rsaKeyPairGenerator);
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, null, null);
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
        
        public void PopulateDsaKeys()
        {
            var keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var dsaKeyProvider = new DsaKeyProvider(keyPairGenerator);
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, dsaKeyProvider, null);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            
            IAsymmetricKeyPair dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
            
            files.Add("private.dsa.der", dsaKeyPair.PrivateKey.Content);
            files.Add("public.dsa.der", dsaKeyPair.PublicKey.Content);
            files.Add("private.dsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(dsaKeyPair.PrivateKey)));
            files.Add("public.dsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(dsaKeyPair.PublicKey)));
            files.Add("private.dsa.encrypted.der", encryptionProvider.EncryptPrivateKey(dsaKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.dsa.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(dsaKeyPair.PrivateKey, "foobarbaz"))));
        }
        
        public void PopulateEcKeys()
        {
            var keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var ecKeyProvider = new EcKeyProvider(keyPairGenerator);
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, ecKeyProvider);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            
            IAsymmetricKeyPair ecKeyPair = ecKeyProvider.CreateKeyPair("b-409");
            
            files.Add("private.ec.der", ecKeyPair.PrivateKey.Content);
            files.Add("public.ec.der", ecKeyPair.PublicKey.Content);
            files.Add("private.ec.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(ecKeyPair.PrivateKey)));
            files.Add("public.ec.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(ecKeyPair.PublicKey)));
            files.Add("private.ec.encrypted.der", encryptionProvider.EncryptPrivateKey(ecKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.ec.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(ecKeyPair.PrivateKey, "foobarbaz"))));
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
            public void ShouldIndicateWhenNoKeysAreGiven()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"--convert", "-t", "der"}); });
                Assert.AreEqual("No keys were specified for conversion.", exception.Message);
            }

            [Test]
            public void ShouldIndicateWhenKeyAlreadyIsInGivenFormat()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => { Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "pem"}); });
                Assert.AreEqual("The given key private.rsa.pem is already in pem format.", exception.Message);
            }

            [Test]
            public void ShouldIndicateWhenNoTargetTypeIsGiven()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem"}); });
                Assert.AreEqual("Target type for key conversion was not specified.", exception.Message);
            }
        }

        [TestFixture]
        public class ConvertUnencryptedRsaKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemKeyPairToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.rsa.der"], files["private.rsa.pem.der"]);
                CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.pem.der"]);
            }

            [Test]
            public void ShouldConvertPublicPemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.pem.der"]);
            }

            [Test]
            public void ShouldConvertPrivatePemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.rsa.der"], files["private.rsa.pem.der"]);
            }
            
            [Test]
            public void ShouldConvertDerKeyPairToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "--publickey", "public.rsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.rsa.pem"], files["private.rsa.der.pem"]);
                CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.der.pem"]);
            }

            [Test]
            public void ShouldConvertPrivateDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.rsa.pem"], files["private.rsa.der.pem"]);
            }

            [Test]
            public void ShouldConvertPublicDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.der.pem"]);
            }
        }

        [TestFixture]
        public class ConvertEncryptedPrivateRsaKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateRsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.rsa.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.rsa.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertDerToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.encrypted.der", "-p", "foobarbaz", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.rsa.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.rsa.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }

        [TestFixture]
        public class ConvertUnencryptedDsaKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateDsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemKeyPairToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "--publickey", "public.dsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.dsa.der"], files["private.dsa.pem.der"]);
                CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.pem.der"]);
            }

            [Test]
            public void ShouldConvertPublicPemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.pem.der"]);
            }

            [Test]
            public void ShouldConvertPrivatePemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.dsa.der"], files["private.dsa.pem.der"]);
            }
            
            [Test]
            public void ShouldConvertDerKeyPairToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "--publickey", "public.dsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.dsa.pem"], files["private.dsa.der.pem"]);
                CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.der.pem"]);
            }

            [Test]
            public void ShouldConvertPrivateDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.dsa.pem"], files["private.dsa.der.pem"]);
            }

            [Test]
            public void ShouldConvertPublicDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.der.pem"]);
            }
        }

        [TestFixture]
        public class ConvertEncryptedPrivateDsaKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateDsaKeys();
            }
            
            [Test]
            public void ShouldConvertPemToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.dsa.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.dsa.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertDerToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.encrypted.der", "-p", "foobarbaz", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.dsa.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.dsa.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }
        
        [TestFixture]
        public class ConvertUnencryptedEcKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateEcKeys();
            }
            
            [Test]
            public void ShouldConvertPemKeyPairToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "--publickey", "public.ec.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.ec.der"], files["private.ec.pem.der"]);
                CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.pem.der"]);
            }

            [Test]
            public void ShouldConvertPublicPemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.ec.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.pem.der"]);
            }

            [Test]
            public void ShouldConvertPrivatePemKeyToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "-t", "der"});
                CollectionAssert.AreEqual(files["private.ec.der"], files["private.ec.pem.der"]);
            }
            
            [Test]
            public void ShouldConvertDerKeyPairToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "--publickey", "public.ec.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.ec.pem"], files["private.ec.der.pem"]);
                CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.der.pem"]);
            }

            [Test]
            public void ShouldConvertPrivateDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["private.ec.pem"], files["private.ec.der.pem"]);
            }

            [Test]
            public void ShouldConvertPublicDerKeyToPem()
            {
                Certifier.Main(new[] {"--convert", "--publickey", "public.ec.der", "-t", "pem"});
                CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.der.pem"]);
            }
        }
        
        [TestFixture]
        public class ConvertEncryptedPrivateEcKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateEcKeys();
            }
            
            [Test]
            public void ShouldConvertPemToDer()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.ec.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.ec.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertDerToPem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.encrypted.der", "-p", "foobarbaz", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.ec.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.ec.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }
    }
}