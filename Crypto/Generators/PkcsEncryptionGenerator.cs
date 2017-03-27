using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Generators
{
    public class PkcsEncryptionGenerator
    {
        public virtual byte[] Encrypt(string password, byte[] salt, int iterationCount, byte[] content)
        {
            var asymmetricKey = PrivateKeyFactory.CreateKey(content);
            return PrivateKeyFactory.EncryptKey(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc, password.ToCharArray(), salt, iterationCount, asymmetricKey);
        }
    }
}