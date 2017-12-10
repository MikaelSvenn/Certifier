using System;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Providers;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class AesKeyEncryptionGeneratorTest
    {
        private AesKeyEncryptionGenerator generator;
        private IAsymmetricKeyPair keyPair;
        private byte[] encryptedKey;
        
        [SetUp]
        public void Setup()
        {
            generator = new AesKeyEncryptionGenerator();
            
            var secureRandom = new SecureRandomGenerator();
            var rsaProvider = new RsaKeyProvider(new AsymmetricKeyPairGenerator(secureRandom));
            keyPair = rsaProvider.CreateKeyPair(1024);
            
            encryptedKey = generator.Encrypt("keypassword", new byte[] {0x01, 0x02}, 100, keyPair.PrivateKey.Content);
        }

        [Test]
        public void ShouldReturnValidResult()
        {
            Assert.DoesNotThrow(() => {EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(encryptedKey));});
        }
        
        [Test]
        public void ShouldReturnEncryptedContent()
        {
            Assert.Throws<InvalidCastException>(() => { PrivateKeyFactory.CreateKey(encryptedKey); });
        }
        
        [Test]
        public void ShouldEncryptUsingSha256WithAes256()
        {
            var encrpytedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(encryptedKey));
            Assert.AreEqual(BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc.Id, encrpytedPrivateKeyInfo.EncryptionAlgorithm.Algorithm.Id);
        }
    }
}