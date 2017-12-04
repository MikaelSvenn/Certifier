using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Crypto.Generators
{
    public class Pkcs12EncryptionGenerator
    {
        public virtual byte[] Encrypt(string password, byte[] salt, int iterationCount, byte[] content)
        {
            AsymmetricKeyParameter asymmetricKey = PrivateKeyFactory.CreateKey(content);
            return PrivateKeyFactory.EncryptKey(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc, password.ToCharArray(), salt, iterationCount, asymmetricKey);
        }
    }
}