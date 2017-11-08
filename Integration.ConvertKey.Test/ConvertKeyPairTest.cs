using System;
using System.Collections.Generic;
using Core.Configuration;
using Core.Interfaces;
using Core.SystemWrappers;
using Crypto.Formatters;
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
        private ISshFormattingProvider sshFormattingProvider;
        
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
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, null, null, null);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            sshFormattingProvider = new SshFormattingProvider(new SshKeyProvider(encodingWrapper, new Base64Wrapper(), rsaKeyProvider, null, null), encodingWrapper, new Ssh2ContentFormatter(), new Base64Wrapper());
            
            IAsymmetricKeyPair rsaKeyPair = rsaKeyProvider.CreateKeyPair(1024);
            
            files.Add("private.rsa.der", rsaKeyPair.PrivateKey.Content);
            files.Add("public.rsa.der", rsaKeyPair.PublicKey.Content);
            files.Add("private.rsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(rsaKeyPair.PrivateKey)));
            files.Add("public.rsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(rsaKeyPair.PublicKey)));
            files.Add("public.rsa.openssh", encodingWrapper.GetBytes(sshFormattingProvider.GetAsOpenSsh(rsaKeyPair.PublicKey, "openssh-key")));
            files.Add("public.rsa.ssh2", encodingWrapper.GetBytes(sshFormattingProvider.GetAsSsh2(rsaKeyPair.PublicKey, "ssh2-key")));
            files.Add("private.rsa.encrypted.der", encryptionProvider.EncryptPrivateKey(rsaKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.rsa.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(rsaKeyPair.PrivateKey, "foobarbaz"))));
        }
        
        public void PopulateDsaKeys()
        {
            var keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var dsaKeyProvider = new DsaKeyProvider(keyPairGenerator);
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, dsaKeyProvider, null, null);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            sshFormattingProvider = new SshFormattingProvider(new SshKeyProvider(encodingWrapper, new Base64Wrapper(), null, dsaKeyProvider, null), encodingWrapper, new Ssh2ContentFormatter(), new Base64Wrapper());
            
            IAsymmetricKeyPair dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
            
            files.Add("private.dsa.der", dsaKeyPair.PrivateKey.Content);
            files.Add("public.dsa.der", dsaKeyPair.PublicKey.Content);
            files.Add("private.dsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(dsaKeyPair.PrivateKey)));
            files.Add("public.dsa.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(dsaKeyPair.PublicKey)));
            files.Add("public.dsa.openssh", encodingWrapper.GetBytes(sshFormattingProvider.GetAsOpenSsh(dsaKeyPair.PublicKey, "openssh-key")));
            files.Add("public.dsa.ssh2", encodingWrapper.GetBytes(sshFormattingProvider.GetAsSsh2(dsaKeyPair.PublicKey, "ssh2-key")));
            files.Add("private.dsa.encrypted.der", encryptionProvider.EncryptPrivateKey(dsaKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.dsa.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(dsaKeyPair.PrivateKey, "foobarbaz"))));
        }
        
        public void PopulateEcKeys()
        {
            var keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var ecKeyProvider = new EcKeyProvider(keyPairGenerator, new FieldToCurveNameMapper());
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, ecKeyProvider, null);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            sshFormattingProvider = new SshFormattingProvider(new SshKeyProvider(encodingWrapper, new Base64Wrapper(), null, null, ecKeyProvider), encodingWrapper, new Ssh2ContentFormatter(), new Base64Wrapper());
            
            IAsymmetricKeyPair ecKeyPair = ecKeyProvider.CreateKeyPair("prime256v1");
            
            files.Add("private.ec.der", ecKeyPair.PrivateKey.Content);
            files.Add("public.ec.der", ecKeyPair.PublicKey.Content);
            files.Add("private.ec.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(ecKeyPair.PrivateKey)));
            files.Add("public.ec.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(ecKeyPair.PublicKey)));
            files.Add("public.ec.openssh", encodingWrapper.GetBytes(sshFormattingProvider.GetAsOpenSsh(ecKeyPair.PublicKey, "openssh-key")));
            files.Add("public.ec.ssh2", encodingWrapper.GetBytes(sshFormattingProvider.GetAsSsh2(ecKeyPair.PublicKey, "ssh2-key")));
            files.Add("private.ec.encrypted.der", encryptionProvider.EncryptPrivateKey(ecKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.ec.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(ecKeyPair.PrivateKey, "foobarbaz"))));
        }

        public void PopulateElGamalKeys()
        {
            var keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var elGamalKeyProvider = new ElGamalKeyProvider(keyPairGenerator, new Rfc3526PrimeMapper());
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, null, elGamalKeyProvider);
            encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyProvider);
            encodingWrapper = new EncodingWrapper();
            
            IAsymmetricKeyPair elGamalKeyPair = elGamalKeyProvider.CreateKeyPair(2048, true);
            
            files.Add("private.elgamal.der", elGamalKeyPair.PrivateKey.Content);
            files.Add("public.elgamal.der", elGamalKeyPair.PublicKey.Content);
            files.Add("private.elgamal.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(elGamalKeyPair.PrivateKey)));
            files.Add("public.elgamal.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(elGamalKeyPair.PublicKey)));
            files.Add("private.elgamal.encrypted.der", encryptionProvider.EncryptPrivateKey(elGamalKeyPair.PrivateKey, "foobarbaz").Content);
            files.Add("private.elgamal.encrypted.pem", encodingWrapper.GetBytes(pkcs8FormattingProvider.GetAsPem(encryptionProvider.EncryptPrivateKey(elGamalKeyPair.PrivateKey, "foobarbaz"))));
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

            [TestFixture]
            public class Pkcs8Pem : ConvertUnencryptedRsaKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "--publickey", "public.rsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.rsa.der"], files["private.rsa.pem.der"]);
                    CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.pem.der"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.pem.der"]);
                }
                
                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.rsa.der"], files["private.rsa.pem.der"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.pem", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.rsa.openssh"], files["public.rsa.pem.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.pem", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.rsa.ssh2"], files["public.rsa.pem.ssh2"]);
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.pem", "-t", "ssh2"}));
                }
            }

            [TestFixture]
            public class Pkcs8Der : ConvertUnencryptedRsaKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "--publickey", "public.rsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.rsa.pem"], files["private.rsa.der.pem"]);
                    CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.der.pem"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.rsa.pem"], files["private.rsa.der.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.der.pem"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.der", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.rsa.openssh"], files["public.rsa.der.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.der", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.rsa.ssh2"], files["public.rsa.der.ssh2"]);
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.der", "-t", "ssh2"}));
                }
            }

            [TestFixture]
            public class OpenSsh : ConvertUnencryptedRsaKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.openssh", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.openssh.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.openssh", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.openssh.der"]);
                }
            }

            [TestFixture]
            public class Ssh2 : ConvertUnencryptedRsaKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.ssh2", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.rsa.pem"], files["public.rsa.ssh2.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.rsa.ssh2", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.rsa.der"], files["public.rsa.ssh2.der"]);
                }
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
            public void ShouldConvertPkcs8PemToPkcs8Der()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.rsa.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.rsa.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.rsa.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertPkcs8DerToPkcs8Pem()
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

            [TestFixture]
            public class Pkcs8Pem : ConvertUnencryptedDsaKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "--publickey", "public.dsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.dsa.der"], files["private.dsa.pem.der"]);
                    CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.pem.der"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.pem.der"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.dsa.der"], files["private.dsa.pem.der"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.pem", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.dsa.openssh"], files["public.dsa.pem.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.pem", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.dsa.ssh2"], files["public.dsa.pem.ssh2"]);
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.pem", "-t", "ssh2"}));
                }
            }

            [TestFixture]
            public class Pkcs8Der : ConvertUnencryptedDsaKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "--publickey", "public.dsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.dsa.pem"], files["private.dsa.der.pem"]);
                    CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.der.pem"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.dsa.pem"], files["private.dsa.der.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.der.pem"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.der", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.dsa.openssh"], files["public.dsa.der.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.der", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.dsa.ssh2"], files["public.dsa.der.ssh2"]);
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.der", "-t", "ssh2"}));
                }
            }

            [TestFixture]
            public class OpenSsh : ConvertUnencryptedDsaKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.openssh", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.openssh.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.openssh", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.openssh.der"]);
                }
            }

            [TestFixture]
            public class Ssh2 : ConvertUnencryptedDsaKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.ssh2", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.dsa.pem"], files["public.dsa.ssh2.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.dsa.ssh2", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.dsa.der"], files["public.dsa.ssh2.der"]);
                }
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
            public void ShouldConvertPkcs8PemToPkcs8Der()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.dsa.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.dsa.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.dsa.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertPkcs8DerToPkcs8Pem()
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

            [TestFixture]
            public class Pkcs8Pem : ConvertUnencryptedEcKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "--publickey", "public.ec.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.ec.der"], files["private.ec.pem.der"]);
                    CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.pem.der"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.pem.der"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.ec.der"], files["private.ec.pem.der"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.pem", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.ec.openssh"], files["public.ec.pem.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.pem", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.ec.ssh2"], files["public.ec.pem.ssh2"]);
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.pem", "-t", "ssh2"}));
                }
            }
            
            [TestFixture]
            public class Pkcs8Der : ConvertUnencryptedEcKey
            {
                            
                [Test]
                public void ShouldConvertKeyPairToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "--publickey", "public.ec.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.ec.pem"], files["private.ec.der.pem"]);
                    CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.der.pem"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.ec.pem"], files["private.ec.der.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.der.pem"]);
                }
                
                [Test]
                public void ShouldConvertPublicKeyToOpenSsh()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.der", "-t", "openssh"});
                    CollectionAssert.AreEqual(files["public.ec.openssh"], files["public.ec.der.openssh"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToSsh2()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.der", "-t", "ssh2"});
                    CollectionAssert.AreEqual(files["public.ec.ssh2"], files["public.ec.der.ssh2"]);
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "-t", "openssh"}));
                }

                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.der", "-t", "ssh2"}));
                }
            }
            
            [TestFixture]
            public class OpenSsh : ConvertUnencryptedEcKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.openssh", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.openssh.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.openssh", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.openssh.der"]);
                }
            }
            
            [TestFixture]
            public class Ssh2 : ConvertUnencryptedEcKey
            {
                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.ssh2", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.ec.pem"], files["public.ec.ssh2.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.ec.ssh2", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.ec.der"], files["public.ec.ssh2.der"]);
                }
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
            public void ShouldConvertPkcs8PemToPkcs8Der()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.ec.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.ec.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertPkcs8DerToPkcs8Pem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.ec.encrypted.der", "-p", "foobarbaz", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.ec.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.ec.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }
        
        
        [TestFixture]
        public class ConvertUnencryptedElGamalKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateElGamalKeys();
            }

            [TestFixture]
            public class Pkcs8Pem : ConvertUnencryptedElGamalKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.pem", "--publickey", "public.elgamal.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.elgamal.der"], files["private.elgamal.pem.der"]);
                    CollectionAssert.AreEqual(files["public.elgamal.der"], files["public.elgamal.pem.der"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["public.elgamal.der"], files["public.elgamal.pem.der"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Der()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.pem", "-t", "der"});
                    CollectionAssert.AreEqual(files["private.elgamal.der"], files["private.elgamal.pem.der"]);
                }

                [Test]
                public void ShouldNotConvertPublicKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.pem", "-t", "openssh"}));
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.pem", "-t", "openssh"}));
                }
                
                [Test]
                public void ShouldNotConvertPublicKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.pem", "-t", "ssh2"}));
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "public.elgamal.pem", "-t", "ssh2"}));
                }
            }
            
            [TestFixture]
            public class Pkcs8Der : ConvertUnencryptedElGamalKey
            {
                [Test]
                public void ShouldConvertKeyPairToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.der", "--publickey", "public.elgamal.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.elgamal.pem"], files["private.elgamal.der.pem"]);
                    CollectionAssert.AreEqual(files["public.elgamal.pem"], files["public.elgamal.der.pem"]);
                }

                [Test]
                public void ShouldConvertPrivateKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["private.elgamal.pem"], files["private.elgamal.der.pem"]);
                }

                [Test]
                public void ShouldConvertPublicKeyToPkcs8Pem()
                {
                    Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.der", "-t", "pem"});
                    CollectionAssert.AreEqual(files["public.elgamal.pem"], files["public.elgamal.der.pem"]);
                }
                
                [Test]
                public void ShouldNotConvertPublicKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.der", "-t", "openssh"}));
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToOpenSsh()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.der", "-t", "openssh"}));
                }
                
                [Test]
                public void ShouldNotConvertPublicKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--publickey", "public.elgamal.der", "-t", "ssh2"}));
                }
                
                [Test]
                public void ShouldNotConvertPrivateKeyToSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.der", "-t", "ssh2"}));
                }
            }
        }
        
        [TestFixture]
        public class ConvertEncryptedPrivateElGamalKey : ConvertKeyPairTest
        {
            [SetUp]
            public void Setup()
            {
                PopulateElGamalKeys();
            }
            
            [Test]
            public void ShouldConvertPkcs8PemToPkcs8Der()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.encrypted.pem", "-p", "foobarbaz", "-t", "der"});
                IAsymmetricKey encryptedKey = asymmetricKeyProvider.GetEncryptedPrivateKey(files["private.elgamal.encrypted.pem.der"]);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.elgamal.der"], decryptedKey.Content);
            }
            
            [Test]
            public void ShouldConvertPkcs8DerToPkcs8Pem()
            {
                Certifier.Main(new[] {"--convert", "--privatekey", "private.elgamal.encrypted.der", "-p", "foobarbaz", "-t", "pem"});
                
                string keyContent = encodingWrapper.GetString(files["private.elgamal.encrypted.der.pem"]);
                IAsymmetricKey encryptedKey = pkcs8FormattingProvider.GetAsDer(keyContent);
                IAsymmetricKey decryptedKey = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobarbaz");
                
                CollectionAssert.AreEqual(files["private.elgamal.pem"], pkcs8FormattingProvider.GetAsPem(decryptedKey));
            }
        }
    }
}