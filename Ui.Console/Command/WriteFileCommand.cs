﻿using Ui.Console.Startup;

namespace Ui.Console.Command
{
    public class WriteFileCommand<T> : ICommandWithOutput<T>
    {
        public byte[] FileContent { get; set; }
        public string FilePath { get; set; }
        public T Out { get; set; }
        public ContentType ContentType { get; set; }
    }
}