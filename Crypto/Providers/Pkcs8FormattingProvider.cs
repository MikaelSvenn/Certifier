using System;
using System.IO;
using Core.Interfaces;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace Crypto.Providers
{
    public class Pkcs8FormattingProvider : IPkcsFormattingProvider<IAsymmetricKey>
    {
        private readonly AsymmetricKeyProvider keyProvider;


        public Pkcs8FormattingProvider(AsymmetricKeyProvider keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public IAsymmetricKey GetAsDer(string key)
        {
            var pemReader = new PemReader(new StringReader(key));
            var pemObject = pemReader.ReadPemObject();

            if (pemObject.Type.Contains("PUBLIC"))
            {
                return keyProvider.GetPublicKey(pemObject.Content);
            }

            return pemObject.Type.Contains("ENCRYPTED") ? keyProvider.GetEncryptedPrivateKey(pemObject.Content) : keyProvider.GetPrivateKey(pemObject.Content);
        }


        public string GetAsPem(IAsymmetricKey key)
        {
            string pemDescriptor;
            if (key.IsPrivateKey)
            {
                pemDescriptor = key.IsEncrypted ? "ENCRYPTED PRIVATE KEY" : "PRIVATE KEY";
            }
            else
            {
                pemDescriptor = "PUBLIC KEY";
            }

            var pemObject = new PemObject(pemDescriptor, key.Content);
            var pemWriter = new PemWriter(new StringWriter());
            pemWriter.WriteObject(pemObject);

            return pemWriter.Writer.ToString();
        }
    }
}