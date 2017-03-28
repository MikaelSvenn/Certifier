using System;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class PkcsEncryptionGeneratorTest
    {
        private PkcsEncryptionGenerator encryptionGenerator;
        private IAsymmetricKeyPair keyPair;
        private byte[] encryptedKey;

        [OneTimeSetUp]
        public void SetupPkcsEncryptionGeneratorTest()
        {
            encryptionGenerator = new PkcsEncryptionGenerator();

            var secureRandom = new SecureRandomGenerator();
            var rsaProvider = new RsaKeyProvider(new RsaKeyPairGenerator(secureRandom));
            keyPair = rsaProvider.CreateKeyPair(1024);

            encryptedKey = encryptionGenerator.Encrypt("fooBar", new byte[] {0x01, 0x02}, 10, keyPair.PrivateKey.Content);
        }

        [Test]
        public void ShouldReturnValidAsn1Result()
        {
            Assert.DoesNotThrow(() => {EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(encryptedKey));});
        }

        [Test]
        public void ShouldEncryptUsingPbe2ShaWith3Key3DesCbc()
        {
            var encrpytedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(encryptedKey));
            Assert.AreEqual(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc.Id, encrpytedPrivateKeyInfo.EncryptionAlgorithm.Algorithm.Id);
        }

        [Test]
        public void ShouldReturnEncryptedContent()
        {
            Assert.Throws<InvalidCastException>(() => { PrivateKeyFactory.CreateKey(encryptedKey); });
        }
    }
}