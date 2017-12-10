using System;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Crypto.Providers
{
    public class KeyEncryptionProvider : IKeyEncryptionProvider
    {
        private readonly IConfiguration configuration;
        private readonly SecureRandomGenerator secureRandomGenerator;
        private readonly IAsymmetricKeyProvider keyProvider;
        private readonly Pkcs12KeyEncryptionGenerator pkcsEncryptionGenerator;
        private readonly AesKeyEncryptionGenerator aesEncryptionGenerator;

        public KeyEncryptionProvider(IConfiguration configuration, SecureRandomGenerator secureRandomGenerator, IAsymmetricKeyProvider keyProvider, Pkcs12KeyEncryptionGenerator pkcsEncryptionGenerator, AesKeyEncryptionGenerator aesEncryptionGenerator)
        {
            this.configuration = configuration;
            this.secureRandomGenerator = secureRandomGenerator;
            this.keyProvider = keyProvider;
            this.pkcsEncryptionGenerator = pkcsEncryptionGenerator;
            this.aesEncryptionGenerator = aesEncryptionGenerator;
        }

        public virtual IAsymmetricKey EncryptPrivateKey(IAsymmetricKey key, string password, EncryptionType encryptionType)
        {
            if (key.IsEncrypted)
            {
                throw new InvalidOperationException("Key is already encrypted");
            }

            if (encryptionType == EncryptionType.None)
            {
                throw new InvalidOperationException("Key encryption type must be specified.");
            }
            
            var saltLength = configuration.Get<int>("SaltLengthInBytes");
            byte[] salt = secureRandomGenerator.NextBytes(saltLength);

            var iterationCount = configuration.Get<int>("KeyDerivationIterationCount");
            
            byte[] privateKeyContent;
            if (encryptionType == EncryptionType.Pkcs)
            {
                privateKeyContent = pkcsEncryptionGenerator.Encrypt(password, salt, iterationCount, key.Content);
            }
            else
            {
                privateKeyContent = aesEncryptionGenerator.Encrypt(password, salt, iterationCount, key.Content);
            }

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