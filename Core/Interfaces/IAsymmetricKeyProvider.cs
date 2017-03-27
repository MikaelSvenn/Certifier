using System;
using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKeyProvider<out T> where T : IAsymmetricKey
    {
        IAsymmetricKeyPair CreateKeyPair(int keySize);

        [Obsolete("TODO: Remove this and utilize only through pkcsEncryptionProvider")]
        IAsymmetricKeyPair CreatePkcs12KeyPair(string password, int keySize);

        T GetKey(byte[] content, AsymmetricKeyType keyType);
    }
}