using Core.Interfaces;

namespace Core.Model
{
    public class EcKey : AsymmetricKey, IEcKey 
    {
        public EcKey(byte[] content, AsymmetricKeyType keyType, int keyLength, string curve) : base(content, keyType, keyLength, CipherType.Ec)
        {
            Curve = curve;
        }
        
        public string Curve { get; }
        public bool IsCurve25519 => Curve == "curve25519";
    }
}