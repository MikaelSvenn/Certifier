using System.IO;

namespace Core.SystemWrappers
{
    public class FileWrapper
    {
        public virtual void WriteAllBytes(string path, byte[] content)
        {
            File.WriteAllBytes(path, content);
        }
        
        public virtual byte[] ReadAllBytes(string filePath) => File.ReadAllBytes(filePath);
    }
}