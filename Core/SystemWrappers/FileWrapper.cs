using System.IO;

namespace Core.SystemWrappers
{
    public class FileWrapper
    {
        public virtual void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public virtual string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public virtual byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}