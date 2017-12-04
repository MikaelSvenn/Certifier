using System;
using Core.Interfaces;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Providers
{
    public class Pkcs8EncryptionProvider : IKeyEncryptionProvider
    {
        private readonly IConfiguration configuration;
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly IAsymmetricKeyProvider keyProvider;
        private readonly Pkcs12EncryptionGenerator encryptionGenerator;

        public Pkcs8EncryptionProvider(IConfiguration configuration, SecureRandomGenerator secureRandomGenerator, IAsymmetricKeyProvider keyProvider, Pkcs12EncryptionGenerator encryptionGenerator)
        {
            this.configuration = configuration;
            this.secureRandomGenerator = secureRandomGenerator;
            this.keyProvider = keyProvider;
            this.encryptionGenerator = encryptionGenerator;
        }

        public virtual IAsymmetricKey EncryptPrivateKey(IAsymmetricKey key, string password)
        {
            if (key.IsEncrypted)
            {
                throw new InvalidOperationException("Key is already encrypted");
            }

            var saltLength = configuration.Get<int>("SaltLengthInBytes");
            byte[] salt = secureRandomGenerator.NextBytes(saltLength);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            byte[] privateKeyContent = encryptionGenerator.Encrypt(password, salt, iterationCount, key.Content);

            return keyProvider.GetEncryptedPrivateKey(privateKeyContent);
        }

        public IAsymmetricKey DecryptPrivateKey(IAsymmetricKey key, string password)
        {
            AsymmetricKeyParameter asymmetricKey;

            try
            {
                asymmetricKey = PrivateKeyFactory.DecryptKey(password.ToCharArray(), key.Content);
            }
            catch (InvalidCipherTextException)
            {
                throw new ArgumentException("Incorrect password was provided or the key is corrupt.");
            }

            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricKey);
            byte[] privateKey = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            return keyProvider.GetPrivateKey(privateKey);
        }
    }
}