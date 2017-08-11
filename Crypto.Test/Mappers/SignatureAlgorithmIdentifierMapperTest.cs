using System;
using Core.Model;
using Crypto.Mappers;
using NUnit.Framework;

namespace Crypto.Test.Mappers
{
    [TestFixture]
    public class SignatureAlgorithmIdentifierMapperTest
    {
        private SignatureAlgorithmIdentifierMapper identifierMapper;

        [SetUp]
        public void SetupSignatureAlgorithmMapperTest()
        {
            identifierMapper = new SignatureAlgorithmIdentifierMapper();
        }

        [TestFixture]
        public class MapToAlgorithmIdentifierIdentifier : SignatureAlgorithmIdentifierMapperTest
        {
            [TestCase(CipherType.Ec, TestName = "EC")]
            [TestCase(CipherType.ElGamal, TestName = "ElGamal")]
            [TestCase(CipherType.Pkcs5Encrypted, TestName = "Pkcs5Encrypted")]
            [TestCase(CipherType.Pkcs12Encrypted, TestName = "Pkcs12Encrypted")]
            [TestCase(CipherType.Unknown, TestName = "Unknown")]
            public void ShouldThrowExceptionWhenUnidentifiedCipherTypeIsGiven(CipherType cipherType)
            {
                Assert.Throws<ArgumentException>(() => { identifierMapper.MapToAlgorithmIdentifier(cipherType); });
            }

            [TestCase(CipherType.Rsa, ExpectedResult = "SHA-512withRSAandMGF1", TestName = "RSA")]
            [TestCase(CipherType.Dsa, ExpectedResult = "2.16.840.1.101.3.4.3.4", TestName = "DSA")]
            public string ShouldReturnAlgorithmFor(CipherType cipherType)
            {
                return identifierMapper.MapToAlgorithmIdentifier(cipherType);
            }
        }
    }
}