using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Crypto.Wrappers
{
    public class KeyInfoWrapper
    {
        public virtual SubjectPublicKeyInfo GetPublicKeyInfo(byte[] content) => SubjectPublicKeyInfo.GetInstance(content);
        public virtual PrivateKeyInfo GetPrivateKeyInfo(byte[] content) => PrivateKeyInfo.GetInstance(content);
    }
}