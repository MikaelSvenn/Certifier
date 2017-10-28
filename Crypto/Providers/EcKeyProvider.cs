using System;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace Crypto.Providers
{
    public class EcKeyProvider : BCKeyProvider, IEcKeyProvider
    {
        private readonly AsymmetricKeyPairGenerator keyPairGenerator;
        
        public EcKeyProvider(AsymmetricKeyPairGenerator keyPairGenerator)
        {
            this.keyPairGenerator = keyPairGenerator;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            throw new InvalidOperationException("EC key must be defined by curve.");
        }

        public IAsymmetricKeyPair CreateKeyPair(string curve)
        {
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateECKeyPair(curve);
            byte[] publicKeyContent = GetPublicKey(keyPair.Public);
            byte[] privateKeyContent = GetPrivateKey(keyPair.Private);
            
            var publicKey = new EcKey(publicKeyContent, AsymmetricKeyType.Public, GetKeyLength(keyPair.Public), curve);
            var privateKey = new EcKey(privateKeyContent, AsymmetricKeyType.Private, GetKeyLength(keyPair.Private), curve);
            
            return new AsymmetricKeyPair(privateKey, publicKey);
        }

        private int GetKeyLength(AsymmetricKeyParameter key) => ((ECKeyParameters) key).Parameters.Curve.FieldSize;
        
        public EcKey GetKey(byte[] content, AsymmetricKeyType keyType)
        {           
            AsymmetricKeyParameter key = CreateKey(content, keyType);

            DerObjectIdentifier identifier = key.IsPrivate ? 
                                 PrivateKeyInfoFactory.CreatePrivateKeyInfo(key).PrivateKeyAlgorithm.Algorithm : 
                                 SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).AlgorithmID.Algorithm;
            
            string curve  = ECNamedCurveTable.GetName(identifier) ?? CustomNamedCurves.GetName(identifier) ?? "unknown";          
            int keyLength = GetKeyLength(key);
            
            //TODO: Bouncy castle supports only one-way mapping.
            //The curve name reverse mapping needs to be done manually by comparing the EC field parameters.
            return new EcKey(content, keyType, keyLength, curve);
        }

        public bool VerifyKeyPair(IAsymmetricKeyPair keyPair)
        {
            ECPrivateKeyParameters privateKey;
            ECPublicKeyParameters publicKey;

            try
            {
                privateKey = GetKeyParameter<ECPrivateKeyParameters>(keyPair.PrivateKey?.Content, AsymmetricKeyType.Private);
                publicKey = GetKeyParameter<ECPublicKeyParameters>(keyPair.PublicKey?.Content, AsymmetricKeyType.Public);
            }
            catch (CryptographicException)
            {
                return false;
            }

            return privateKey.Parameters.Equals(publicKey.Parameters) &&
                   publicKey.Parameters.Curve.A.FieldName == privateKey.Parameters.Curve.A.FieldName &&
                   publicKey.Q.Equals(privateKey.Parameters.G.Multiply(privateKey.D));
        }
    }
}