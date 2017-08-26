using System;
using System.Collections.Generic;
using Core.Model;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;

namespace Crypto.Mappers
{
    public class SignatureAlgorithmIdentifierMapper
    {
        private readonly Dictionary<CipherType, string> bouncyCastleSignatureAlgorithms;

        public SignatureAlgorithmIdentifierMapper()
        {
            bouncyCastleSignatureAlgorithms = new Dictionary<CipherType, string>
            {
                {CipherType.Rsa, "SHA-512withRSAandMGF1"},
                {CipherType.Dsa, NistObjectIdentifiers.DsaWithSha512.Id},
                {CipherType.Ec, X9ObjectIdentifiers.ECDsaWithSha512.Id}
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