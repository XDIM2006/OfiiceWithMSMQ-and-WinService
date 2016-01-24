using System.Collections.Concurrent;
using System.Configuration;
using MultiThreadResizer;
using System;

namespace WebApplication1.Models
{
    public class workerClass
    {
        private static int _maxTaskCount;
        private static int MaxTaskCount {
            get {
                if (_maxTaskCount == 0) {
                    int maxTaskCount;
                    int.TryParse(ConfigurationManager.AppSettings["maxTaskCount"], out maxTaskCount);
                    _maxTaskCount = Math.Max(maxTaskCount, 1);
                }
                return _maxTaskCount;
            }
        }
        private static int _maxImagesCountinOneThread;
        private static int MaxImagesCountinOneThread
        {
            get
            {
                if (_maxImagesCountinOneThread == 0)
                {
                    int maxImagesCountinOneThread;
                    int.TryParse(ConfigurationManager.AppSettings["maxImagesCountinOneThread"], out maxImagesCountinOneThread);
                    _maxImagesCountinOneThread = Math.Max(maxImagesCountinOneThread, 1);
                }
                return _maxImagesCountinOneThread;
            }
        }
        private static int _previewSize;
        private static int PreviewSize
        {
            get
            {
                if (_previewSize == 0)
                {
                    int previewSize;
                    int.TryParse(ConfigurationManager.AppSettings["previewSize"], out previewSize);
                    _previewSize = Math.Max(previewSize, 5);
                }
                return _previewSize;
            }
        }
        private static string _previewName;
        public static string PreviewName
        {
            get
            {
                if (string.IsNullOrEmpty(_previewName))
                {
                    _previewName = ConfigurationManager.AppSettings["previewName"];
                }
                return _previewName;
            }
        }

        private static ConcurrentDictionary<string, MultiThreadResizerWorker> workers =
            new ConcurrentDictionary<string, MultiThreadResizerWorker>();

        public static MultiThreadResizerWorker GetWorker(string uuid)
        {
            return workers.GetOrAdd(uuid, (s) => {
                int maxTaskCount;
                int.TryParse(ConfigurationManager.AppSettings["maxTaskCount"], out maxTaskCount);
                int maxImagesCountinOneThread;
                int.TryParse(ConfigurationManager.AppSettings["maxImagesCountinOneThread"], out maxImagesCountinOneThread);
                var worker = new MultiThreadResizerWorker(MaxTaskCount, MaxImagesCountinOneThread);
                worker.ListOfResizeSettings.Add(
                    new CustomResizeSettings(PreviewName, PreviewSize, PreviewSize));
                return worker;
            });
        }

     //   public static MultiThreadResizerWorker Worker = new MultiThreadResizerWorker(2, 10);
    }
}