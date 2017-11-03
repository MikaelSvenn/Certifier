using Core.SystemWrappers;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class SshKeyProviderTest
    {
        private SshKeyProvider provider;

        [SetUp]
        public void SetupSshKeyProviderTest()
        {
            provider = new SshKeyProvider(new EncodingWrapper(), new Base64Wrapper());
        }

        [TestFixture]
        public class IsSupportedCurve : SshKeyProviderTest
        {
            [TestCase("curve25519")]
            [TestCase("P-256")]
            [TestCase("secp256r1")]
            [TestCase("prime256v1")]
            [TestCase("P-384")]
            [TestCase("secp384r1")]
            [TestCase("P-521")]
            [TestCase("secp521r1")]
            public void ShouldReturnTrueForSshSupportedCurve(string curve)
            {
                Assert.IsTrue(provider.IsSupportedCurve(curve));
            }

            [TestCase("brainpoolP384t1")]
            [TestCase("prime239v3")]
            [TestCase("c2tnb191v3")]
            [TestCase("FRP256v1")]
            public void ShouldReturnFalseForNotSupportedCurve(string curve)
            {
                Assert.IsFalse(provider.IsSupportedCurve(curve));
            }
        }

        [TestFixture]
        public class GetCurveSshHeader : SshKeyProviderTest
        {
            [TestCase("curve25519", ExpectedResult = "ssh-ed25519")]
            [TestCase("P-256", ExpectedResult = "ecdsa-sha2-nistp256")]
            [TestCase("P-384", ExpectedResult = "ecdsa-sha2-nistp384")]
            [TestCase("P-521", ExpectedResult = "ecdsa-sha2-nistp521")]
            public string ShouldReturnSshHeaderForGivenCurve(string curve) => provider.GetCurveSshHeader(curve);
        }
    }
}