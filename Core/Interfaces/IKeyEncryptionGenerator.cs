namespace Core.Interfaces {
    public interface IKeyEncryptionGenerator {
        byte[] Encrypt(string password, byte[] salt, int iterationCount, byte[] content);
    }
}