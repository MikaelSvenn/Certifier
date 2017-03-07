using System.Net;
using Core.Model;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Core.Test.Model
{
    [TestFixture]
    public class RsaKeyTest
    {
        [TestFixture]
        public class IsEncrypted : RsaKeyTest
        {
            [Test]
            public void ShouldRetrunFalseWhenKeyTypeIsRsa()
            {
                var key = new RsaKey(null, AsymmetricKeyType.Rsa, 1);
                Assert.IsFalse(key.IsEncrypted);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsRsaPkcs12()
            {
                var key = new RsaKey(null, AsymmetricKeyType.RsaPkcs12, 1);
                Assert.IsTrue(key.IsEncrypted);
            }
        }
    }
}