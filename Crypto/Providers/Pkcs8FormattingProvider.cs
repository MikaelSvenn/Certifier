using System;
using Core.Interfaces;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace Crypto.Providers
{
    public class Pkcs8FormattingProvider : IPkcsFormattingProvider<IAsymmetricKey>
    {
        public IAsymmetricKey GetAsDer(string key)
        {
            throw new NotImplementedException();
        }

        public string GetAsPem(IAsymmetricKey key)
        {
            //PKCS8 format only contains the ---BEGIN ENCRYPTED PRIVATE KEY... : all the required information is stored in the ASN.1 of PKCS8 key.
            string pemDescriptor = key.IsEncrypted ? "ENCRYPTED PRIVATE KEY" : "PRIVATE KEY";
            var pemObject = new PemObject(pemDescriptor, key.Content);

            throw new NotImplementedException();
        }
    }
}