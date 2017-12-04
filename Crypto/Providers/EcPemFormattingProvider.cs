using System;
using System.IO;
using Core.Interfaces;
using Org.BouncyCastle.Utilities.IO.Pem;
using PemWriter = Org.BouncyCastle.OpenSsl.PemWriter;

namespace Crypto.Providers
{
    public class EcPemFormattingProvider : IPemFormattingProvider<IEcKey>
    {
        private readonly IEcKeyProvider keyProvider;
        public EcPemFormattingProvider(IEcKeyProvider keyProvider)
        {
            this.keyProvider = keyProvider;
        }

        public IEcKey GetAsDer(string ecKey)
        {
            var pemReader = new PemReader(new StringReader(ecKey));
            PemObject pemObject = pemReader.ReadPemObject();

            if (pemObject.Type.Contains("PUBLIC"))
            {
                throw new InvalidOperationException("EC key format not supported.");
            }

            if (ecKey.Contains("ENCRYPTED"))
            {
                throw new InvalidOperationException("Encrypted SEC1 EC key format is not supported.");
            }

            return keyProvider.GetSec1PrivateKeyAsPkcs8(pemObject.Content);
        }

        public string GetAsPem(IEcKey key)
        {
            string keyHeader = key.IsPrivateKey ? "EC PRIVATE KEY" : "PUBLIC KEY";
            var pemObject = new PemObject(keyHeader, key.Content);
            var pemWriter = new PemWriter(new StringWriter());
            pemWriter.WriteObject(pemObject);
            
            return pemWriter.Writer.ToString();
        }
    }
}