using Core.Model;

namespace Core.Services
{
    public interface IKeyService
    {
        RsaKeyPair CreateRsaKeyPair(string password, int keySizeInBits = 4096);
    }
}