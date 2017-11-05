using Core.Model;

namespace Core.Interfaces
{
    public interface IDsaKeyProvider : IKeyProvider<DsaKey>
    {
        DsaKey GetPublicKey(byte[] p, byte[] q, byte[] g, byte[] y);
    }
}