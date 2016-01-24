using System.Collections.Generic;
using MultiThreadResizer;
using System;

namespace WebApplication1.Models
{
    [Serializable]
    public class WorkModel
    {
        public ImageModel OriginalFile { get; set; }
        public ImageModel PreviewFile { get; set; }
        public List<ImageModel> Files { get; set; }
        public WorkModel()
        {
            Files = new List<ImageModel>();
        }
    }
    [Serializable]
    public class ImageModel
    {
        public string filePath { get; set; }
        public string fileName { get; set; }
        public string StartTime { get; }
        public string FinishTime { get; }
        public ImageModel()
        {

        }
        public ImageModel(StatusOfImage status)
        {
            StartTime = status.StartTime.ToString("F");
            FinishTime = status.FinishTime.ToString("F");
        }
    }

    public class CustomResizeSettingsModel
    {
        public static List<CustomResizeSettings> CustomResizeSettings { get; set; }
    }

    public class ResizeSettingsModel
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

}