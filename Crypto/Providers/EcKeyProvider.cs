﻿using System;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;

namespace Crypto.Providers
{
    public class EcKeyProvider : BCKeyProvider, IEcKeyProvider
    {
        private readonly AsymmetricKeyPairGenerator keyPairGenerator;
        private readonly FieldToCurveNameMapper curveNameMapper;

        public EcKeyProvider(AsymmetricKeyPairGenerator keyPairGenerator, FieldToCurveNameMapper curveNameMapper)
        {
            this.keyPairGenerator = keyPairGenerator;
            this.curveNameMapper = curveNameMapper;
        }

        public IAsymmetricKeyPair CreateKeyPair(int keySize)
        {
            throw new InvalidOperationException("EC key must be defined by curve.");
        }

        public IAsymmetricKeyPair CreateKeyPair(string curve)
        {
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateEcKeyPair(curve);
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
            int keyLength = GetKeyLength(key);

            ECCurve curve = ((ECKeyParameters) key).Parameters.Curve;
            string curveName = curveNameMapper.MapCurveToName(curve);

            return new EcKey(content, keyType, keyLength, curveName);
        }

        // Based on RFCs 5480 and 5915, a named curve is used whenever possible.
        public IEcKey GetPublicKey(byte[] q, string curve)
        {
            ECPublicKeyParameters ecPublicKeyParameter;
            if (curve == "curve25519")
            {
                ecPublicKeyParameter = GetNonStandardCurve(q, curve);
            }
            else
            {
                DerObjectIdentifier curveOid = ECNamedCurveTable.GetOid(curve) ?? CustomNamedCurves.GetOid(curve);
                X9ECParameters curveParameters = CustomNamedCurves.GetByOid(curveOid) ?? ECNamedCurveTable.GetByOid(curveOid);
            
                ECPoint qPoint = curveParameters.Curve.DecodePoint(q);
                ecPublicKeyParameter = new ECPublicKeyParameters("EC", qPoint, curveOid);;
            }
            
            byte[] publicKeyContent = GetPublicKey(ecPublicKeyParameter);
            int keyLength = GetKeyLength(ecPublicKeyParameter);
            string curveName = curveNameMapper.MapCurveToName(ecPublicKeyParameter.Parameters.Curve);
            
            return new EcKey(publicKeyContent, AsymmetricKeyType.Public, keyLength, curveName);
        }

        private ECPublicKeyParameters GetNonStandardCurve(byte[] q, string curve)
        {
            X9ECParameters curveParameters = ECNamedCurveTable.GetByName(curve) ?? CustomNamedCurves.GetByName(curve);
            var ecDomainParameters = new ECDomainParameters(curveParameters.Curve,
                                                            curveParameters.G,
                                                            curveParameters.N,
                                                            curveParameters.H,
                                                            curveParameters.GetSeed());

            ECPoint qPoint = ecDomainParameters.Curve.DecodePoint(q);
            return new ECPublicKeyParameters(qPoint, ecDomainParameters);
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