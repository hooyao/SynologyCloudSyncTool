using System;
using com.hy.synology.filemanager.core.exception;

namespace com.hy.synolocgy.filemanager.simplecli
{
    public class ConsoleExceptionHandler:IExceptionHandler
    {
        public void Handle(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}