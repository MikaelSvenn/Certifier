using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class EcPemFormattingProviderTest
    {
        private EcPemFormattingProvider provider;
        private IAsymmetricKeyPair keyPair;
        private EcKeyProvider ecKeyProvider;

        [SetUp]
        public void SetupEcPemFormattingProviderTest()
        {
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            ecKeyProvider = new EcKeyProvider(asymmetricKeyPairGenerator, new FieldToCurveNameMapper());
            provider = new EcPemFormattingProvider(ecKeyProvider);
            keyPair = ecKeyProvider.CreateKeyPair("P-256");
        }
        
        [TestFixture]
        public class GetAsPem : EcPemFormattingProviderTest
        {
            [Test]
            public void ShouldFormatPrivateKeyToSec1Pem()
            {
                string result = provider.GetAsPem((IEcKey)keyPair.PrivateKey);
                string[] resultLines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                Assert.AreEqual(4, resultLines.Length);
                Assert.AreEqual("-----BEGIN EC PRIVATE KEY-----", resultLines.First());
                Assert.AreEqual("-----END EC PRIVATE KEY-----", resultLines.Last());
            }

            [Test]
            public void ShouldFormatPublicKey()
            {
                string result = provider.GetAsPem((IEcKey)keyPair.PublicKey);
                string[] resultLines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                Assert.AreEqual(4, resultLines.Length);
                Assert.AreEqual("-----BEGIN PUBLIC KEY-----", resultLines.First());
                Assert.AreEqual("-----END PUBLIC KEY-----", resultLines.Last());
            }
        }

        [TestFixture]
        public class GetAsDer : EcPemFormattingProviderTest
        {
            private string privateKey;
            private string publicKey;
            private IEcKey sec1PrivateKey;

            [SetUp]
            public void Setup()
            {
                sec1PrivateKey = ecKeyProvider.GetPkcs8PrivateKeyAsSec1((IEcKey) keyPair.PrivateKey);
                privateKey = provider.GetAsPem(sec1PrivateKey);
                publicKey = provider.GetAsPem((IEcKey) keyPair.PublicKey);
            }
            
            [Test]
            public void ShouldThrowExceptionWhenFormattingPublicKey()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => provider.GetAsDer(publicKey));
                Assert.AreEqual("EC key format not supported.", exception.Message);
            }

            [Test]
            public void ShouldThrowExceptionWhenFormattingEncryptedKey()
            {
                string[] resultLines = privateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var content = new List<string>
                {
                    resultLines[0],
                    "Proc-Type: 4,ENCRYPTED",
                    "DEK-Info: AES-256-CBC,FOOBARBAZ",
                    resultLines[1],
                    resultLines[2],
                    resultLines[3]
                };

                var exception = Assert.Throws<InvalidOperationException>(() => provider.GetAsDer(string.Join("\r\n", content)));
                Assert.AreEqual("Encrypted SEC1 EC key format is not supported.", exception.Message);
            }

            [Test]
            public void ShouldReturnValidPkcs8Key()
            {
                IEcKey result = provider.GetAsDer(privateKey);
                Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(result, keyPair.PublicKey)));
            }
        }
    }
}