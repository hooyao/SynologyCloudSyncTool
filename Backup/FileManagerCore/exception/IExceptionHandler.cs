using System;

namespace com.hy.synology.filemanager.core.exception
{
    public interface IExceptionHandler
    {
        void Handle(Exception ex);
    }
}