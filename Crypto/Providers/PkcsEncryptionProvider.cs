using System;
using Core.Interfaces;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Providers
{
    public class PkcsEncryptionProvider
    {
        private readonly IConfiguration configuration;
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly AsymmetricKeyProvider keyProvider;
        private readonly PkcsEncryptionGenerator encryptionGenerator;

        public PkcsEncryptionProvider(IConfiguration configuration, SecureRandomGenerator secureRandomGenerator, AsymmetricKeyProvider keyProvider, PkcsEncryptionGenerator encryptionGenerator)
        {
            this.configuration = configuration;
            this.secureRandomGenerator = secureRandomGenerator;
            this.keyProvider = keyProvider;
            this.encryptionGenerator = encryptionGenerator;
        }

        public IAsymmetricKey EncryptPrivateKey(IAsymmetricKey key, string password)
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
                throw new ArgumentException("The provided password was incorrect");
            }

            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricKey);;
            var privateKey = privateKeyInfo
                .ToAsn1Object()
                .GetDerEncoded();

            return keyProvider.GetPrivateKey(privateKey);
        }
    }
}