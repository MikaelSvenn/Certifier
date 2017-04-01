using System.Collections.Generic;
using Core.Model;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;

namespace Crypto.Mappers
{
    public class OidToCipherTypeMapper
    {
        private readonly Dictionary<string, CipherType> keyAlgorithms;

        public OidToCipherTypeMapper()
        {
            keyAlgorithms = new Dictionary<string, CipherType>
            {
                {PkcsObjectIdentifiers.RsaEncryption.Id, CipherType.Rsa},
                {X509ObjectIdentifiers.IdEARsa.Id, CipherType.Rsa},
                {PkcsObjectIdentifiers.IdRsassaPss.Id, CipherType.Rsa},
                {PkcsObjectIdentifiers.IdRsaesOaep.Id, CipherType.Rsa},
                {X9ObjectIdentifiers.IdDsa.Id, CipherType.Dsa},
                {OiwObjectIdentifiers.ElGamalAlgorithm.Id, CipherType.ElGamal},
                {CryptoProObjectIdentifiers.GostR3410x2001.Id, CipherType.Ec},
                {CryptoProObjectIdentifiers.GostR3410x94.Id, CipherType.Ec},
                {X9ObjectIdentifiers.IdECPublicKey.Id, CipherType.Ec},
                {PkcsObjectIdentifiers.Pkcs5 + ".1", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs5 + ".3", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs5 + ".4", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs5 + ".6", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs5 + ".10", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs5 + ".11", CipherType.Pkcs5Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".1", CipherType.Pkcs12Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".2", CipherType.Pkcs12Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".3", CipherType.Pkcs12Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".4", CipherType.Pkcs12Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".5", CipherType.Pkcs12Encrypted},
                {PkcsObjectIdentifiers.Pkcs12PbeIds + ".6", CipherType.Pkcs12Encrypted}
            };
        }

        public virtual CipherType MapOidToCipherType(string oid)
        {
            if (!keyAlgorithms.ContainsKey(oid))
            {
                return CipherType.Unknown;
            }

            return keyAlgorithms[oid];
        }
    }
}