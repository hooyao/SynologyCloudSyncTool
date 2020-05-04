using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using com.hy.synology.filemanager.core.crypto;
using com.hy.synology.filemanager.core.exception;
using com.hy.synology.filemanager.core.file;

namespace com.hy.synolocgy.filemanager.simplecli
{
    class Program
    {
        private static readonly string CurrentDirectory =
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        private static readonly string KeyFilePath = Path.Join(CurrentDirectory, "key.zip");
        private static readonly string OutputDirectory = Path.Join(CurrentDirectory, "output");

        private static CloudSyncFileProcessorFacade _facade;

        private static void InitBeans()
        {
            HandlerFactory handlerFactory = new HandlerFactory();
            StringHandler stringHandler = new StringHandler();
            IntHandler intHandler = new IntHandler();
            ByteSteamHandler byteSteamHandler = new ByteSteamHandler();
            OrderedDictHandler dictHandler = new OrderedDictHandler(handlerFactory);
            handlerFactory.AddHandler(stringHandler);
            handlerFactory.AddHandler(dictHandler);
            handlerFactory.AddHandler(intHandler);
            handlerFactory.AddHandler(byteSteamHandler);
            CloudSyncKey cloudSyncKey = new CloudSyncKey(KeyFilePath);
            IExceptionHandler exceptionHandler = new ConsoleExceptionHandler();
            _facade = new CloudSyncFileProcessorFacade(handlerFactory, cloudSyncKey, exceptionHandler);
        }

        public static void Main(string[] args)
        {
            //args = new[] {@"Z:\encrypted_jpg_01.jpg"};
            if (!args.Any()) return;
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            //init beans
            InitBeans();

            foreach (var sourceFilePath in args)
            {
                _facade.ProcessFile(sourceFilePath, OutputDirectory);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}