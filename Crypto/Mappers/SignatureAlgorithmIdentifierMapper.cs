using System;
using System.Collections.Generic;
using Core.Model;

namespace Crypto.Mappers
{
    public class SignatureAlgorithmIdentifierMapper
    {
        private readonly Dictionary<CipherType, string> bouncyCastleSignatureAlgorithms;

        public SignatureAlgorithmIdentifierMapper()
        {
            bouncyCastleSignatureAlgorithms = new Dictionary<CipherType, string>
            {
                {CipherType.Rsa, "SHA-512withRSAandMGF1"}
            };
        }

        public string MapToAlgorithmIdentifier(CipherType cipherType)
        {
            if (!bouncyCastleSignatureAlgorithms.ContainsKey(cipherType))
            {
                throw new ArgumentException("Cipher type not supported for signing.");
            }

            return bouncyCastleSignatureAlgorithms[cipherType];
        }
    }
}