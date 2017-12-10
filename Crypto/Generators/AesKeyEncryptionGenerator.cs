using Core.Interfaces;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Crypto.Generators
{
    public class AesKeyEncryptionGenerator : IKeyEncryptionGenerator
    {
        public virtual byte[] Encrypt(string password, byte[] salt, int iterationCount, byte[] content)
        {
            AsymmetricKeyParameter asymmetricKey = PrivateKeyFactory.CreateKey(content);
            byte[] encryptedContent =  PrivateKeyFactory.EncryptKey(BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc, password.ToCharArray(), salt, iterationCount, asymmetricKey);

            var encryptedKeyPrimitives = (DerSequence)Asn1Object.FromByteArray(encryptedContent);
            var derPrimitives = new[]
            {
                new DerSequence(new DerObjectIdentifier(BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc.Id), 
                                ((DerSequence) encryptedKeyPrimitives[0])[1]),
                encryptedKeyPrimitives[1]
            };
            
            var keySequence = new DerSequence(derPrimitives);
            return keySequence.GetDerEncoded();
        }
    }
}