using Org.BouncyCastle.Math;

namespace Crypto.Providers {
    public class AsymmetricKeyParameters {
        public BigInteger Prime { get; set; }
        public BigInteger Generator { get; set; }
    }
}