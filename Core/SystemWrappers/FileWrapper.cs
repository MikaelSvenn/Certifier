using System.IO;

namespace Core.SystemWrappers
{
    public class FileWrapper
    {
        public virtual void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}