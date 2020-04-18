using System;
using System.IO;

namespace com.hy.synology.filemanager.core.crypto
{
    public interface IFileStreamHandler<out T>
    {
        Type ReturnType { get; }
        byte SupportedTag { get; }
        T Handle(BinaryReader br);
    }
}