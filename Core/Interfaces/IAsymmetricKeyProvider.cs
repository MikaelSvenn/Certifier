using System;
using Core.Model;

namespace Core.Interfaces
{
    public interface IAsymmetricKeyProvider<out T> where T : IAsymmetricKey
    {
        IAsymmetricKeyPair CreateKeyPair(int keySize);
        T GetKey(byte[] content, AsymmetricKeyType keyType);
    }
}