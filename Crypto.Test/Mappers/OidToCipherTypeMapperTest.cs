using System.Collections;
using Core.Model;
using Crypto.Mappers;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X9;

namespace Crypto.Test.Mappers
{
    [TestFixture]
    public class OidToCipherTypeMapperTest
    {
        private OidToCipherTypeMapper cipherTypeMapper;

        [SetUp]
        public void Setup()
        {
            cipherTypeMapper = new OidToCipherTypeMapper();
        }

        [Test, TestCaseSource(typeof(OidToCipherTypeTestData), nameof(OidToCipherTypeTestData.OidMappings))]
        public CipherType ShouldMapCipherType(string oid)
        {
            return cipherTypeMapper.MapOidToCipherType(oid);
        }

        [Test]
        public void ShouldMapUnknownOidToUnknown()
        {
            Assert.AreEqual(CipherType.Unknown, cipherTypeMapper.MapOidToCipherType("foo"));
        }
    }

    public class OidToCipherTypeTestData
    {
        public static IEnumerable OidMappings
        {
            get
            {
                yield return new TestCaseData(PkcsObjectIdentifiers.RsaEncryption.Id)
                    .SetName("RSA")
                    .Returns(CipherType.Rsa);

                yield return new TestCaseData(X9ObjectIdentifiers.IdDsa.Id)
                    .SetName("DSA")
                    .Returns(CipherType.Dsa);

                yield return new TestCaseData(CryptoProObjectIdentifiers.GostR3410x2001.Id)
                    .SetName("EC (Gost R3410x2001)")
                    .Returns(CipherType.Ec);

                yield return new TestCaseData(X9ObjectIdentifiers.IdECPublicKey.Id)
                    .SetName("EC (public key)")
                    .Returns(CipherType.Ec);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".1")
                    .SetName("PKCS5 (PBE with MD2 and DES CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".3")
                    .SetName("PKCS5 (PBE with MD5 and DES CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".4")
                    .SetName("PKCS5 (PBE with MD2 and RC2 CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".6")
                    .SetName("PKCS5 (PBE with MD5 and RC2 CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".10")
                    .SetName("PKCS5 (PBE with SHA1 and DES CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs5 + ".11")
                    .SetName("PKCS5 (PBE with SHA1 and RC2 CBC)")
                    .Returns(CipherType.Pkcs5Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".1")
                    .SetName("PKCS12 (PBE with SHA1 and 128bit RC4)")
                    .Returns(CipherType.Pkcs12Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".2")
                    .SetName("PKCS12 (PBE with SHA1 and 40bit RC4)")
                    .Returns(CipherType.Pkcs12Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".3")
                    .SetName("PKCS12 (PBE with SHA1 and 3-key 3DES CBC)")
                    .Returns(CipherType.Pkcs12Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".4")
                    .SetName("PKCS12 (PBE with SHA1 and 2-key 3DES CBC)")
                    .Returns(CipherType.Pkcs12Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".5")
                    .SetName("PKCS12 (PBE with SHA1 and 128bit RC2 CBC)")
                    .Returns(CipherType.Pkcs12Encrypted);

                yield return new TestCaseData(PkcsObjectIdentifiers.Pkcs12PbeIds + ".6")
                    .SetName("PKCS12 (PBE with SHA1 and 40bit RC2 CBC)")
                    .Returns(CipherType.Pkcs12Encrypted);
            }
        }
    }
}