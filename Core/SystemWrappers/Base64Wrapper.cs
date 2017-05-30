namespace Core.SystemWrappers
{
    public class Base64Wrapper
    {
        public virtual string ToBase64String(byte[] content) => System.Convert.ToBase64String(content);
        public virtual byte[] FromBase64String(string content) => System.Convert.FromBase64String(content);
    }
}