using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Mappers
{
    [TestFixture]
    public class FieldToCurveNameMapperTest
    {
        private FieldToCurveNameMapper mapper;
        private EcKeyProvider keyProvider;
        
        [SetUp]
        public void Setup()
        {
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            keyProvider = new EcKeyProvider(asymmetricKeyPairGenerator, new FieldToCurveNameMapper());
            
            mapper = new FieldToCurveNameMapper();
        }

        [Test, TestCaseSource(typeof(FieldNames), nameof(FieldNames.CurveNames))]
        public void ShouldReturnCurveNameForKnownCurve(string curveName)
        {
            var keyPair = keyProvider.CreateKeyPair(curveName);
            var privateKey = PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);

            var parameters = (ECKeyParameters) privateKey;

            string result = mapper.MapCurveToName(parameters.Parameters.Curve);
            if (curveNameSynonyms.Any(synonym => synonym.Contains(result)))
            {
                IEnumerable<string> synonymousCurve = curveNameSynonyms.Single(s => s.Contains(result));
                Assert.IsTrue(synonymousCurve.Contains(curveName));
            }
            else
            {
                Assert.AreEqual(curveName, result);
            }
        }

        [Test]
        public void ShouldReturnUnknownForUnknownCurve()
        {
            ECCurve curve = new FpCurve(new BigInteger("C302F41D932A36CDA7A3463093D18DB78FCE476DE1A8FFFF", 16),
                                       new BigInteger("6A91174076B1E0E19C39C031FE8685C1CAE040E5C69AFFFF", 16),
                                       new BigInteger("469A28EF7C28CCA3DC721D044F4496BCCA7EF4146FBFFFFF", 16),
                                       new BigInteger("C302F41D932A36CDA7A3462F9E9E916B5BE8F1029AC4ACC1", 16),
                                       new BigInteger("01", 16));
            
            Assert.AreEqual("unknown", mapper.MapCurveToName(curve));
        }
        
        //RFC4492 Appendix A.
        private readonly List<IEnumerable<string>> curveNameSynonyms = new List<IEnumerable<string>>
        {
            new []{"sect163k1", "K-163"},
            new []{"sect163r2", "B-163"},
            new []{"sect233k1", "K-233"},
            new []{"sect233r1", "B-233"},
            new []{"sect283k1", "K-283"},
            new []{"sect283r1", "B-283"},
            new []{"sect409k1", "K-409"},
            new []{"sect409r1", "B-409"},
            new []{"sect571k1", "K-571"},
            new []{"sect571r1", "B-571"},
            new []{"secp192r1", "prime192v1", "P-192"},
            new []{"secp224r1", "P-224"},
            new []{"secp256r1", "prime256v1", "P-256"},
            new []{"secp384r1", "P-384"},
            new []{"secp521r1", "P-521"}
        };
        
        public class FieldNames
        {
            public static IEnumerable CurveNames
            {
                get
                {
                    foreach (string curveName in ECNamedCurveTable.Names.Cast<string>())
                    {
                        yield return new TestCaseData(curveName).SetName(curveName);
                    }
                    
                    foreach (string curveName in CustomNamedCurves.Names.Cast<string>())
                    {
                        yield return new TestCaseData(curveName).SetName(curveName);
                    }
                }
            }
        }
    }
}