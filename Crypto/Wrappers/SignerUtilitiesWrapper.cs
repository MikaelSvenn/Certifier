using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Crypto.Wrappers
{
    public class SignerUtilitiesWrapper
    {
        public virtual ISigner GetSigner(string algorithm) => SignerUtilities.GetSigner(algorithm);
    }
}