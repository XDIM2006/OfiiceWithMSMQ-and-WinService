using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using MultiThreadResizer;
using System.Messaging;
using System.Configuration;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(CollectionOfWorkers.SizeOfPage);
        }
        [HttpPost]
        public async Task<JsonResult> Upload()
        {
            var uuid = Request["uuid"];
            ImagesAndSettingsForResizing worker;
            if (!string.IsNullOrEmpty(uuid))
            {
                worker = CollectionOfWorkers.GetWorker(uuid);
            }
            else
            {
                throw new Exception("uuid is not passed");
            }
            List<string> ImagesForWorker = new List<string>(); ;
            List<Image> userImages = GetImagesFromRequest(ref ImagesForWorker);

            worker.ImagesForWorker = ImagesForWorker;

            MessageQueue todoQueue = InitializeMQ(ConfigurationManager.AppSettings["todoQueue"], new XmlMessageFormatter(
                 new Type[] { typeof(ImagesAndSettingsForResizing), typeof(ResizeSettingsModel) }));
            MessageQueue doneQueue = InitializeMQ(ConfigurationManager.AppSettings["doneQueue"], new XmlMessageFormatter(
                new Type[] { typeof(List<ResizedImageInfo>) }));
            using (todoQueue)
            {
                todoQueue.Send(worker);
            }
            List<ResizedImageInfo> ResizedImagesList = null;
            using (doneQueue)
            {
                do
                {
                    Message message = doneQueue.Receive();

                    ResizedImagesList = message.Body as List<ResizedImageInfo>;
                }
                while (ResizedImagesList == null);
            }
            HandledResizedImages(ResizedImagesList, userImages);

            return Json(userImages);
        }

        private MessageQueue InitializeMQ(string MQName, XmlMessageFormatter formatter)
        {
            MessageQueue MQ;

            if (MessageQueue.Exists(MQName))
                MQ = new MessageQueue(MQName);
            else
                MQ = MessageQueue.Create(MQName);

            MQ.Formatter = formatter;

            return MQ;
        }

        private static void SaveImagesInDB(Image userImage)
        {
            using (var db = new ImageContext())
            {
                try
                {
                    db.Images.Add(userImage);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void HandledResizedImages(List<ResizedImageInfo> ResizedImagesList, List<Image> userImages)
        {

            userImages.ForEach(userImage =>
            {
                userImage.PreviewPath = Url.Content("~/Images/" +
                    ResizedImagesList.FirstOrDefault(f =>
                        f.FileSourceName == userImage.FileName
                        && f.FileName.Contains(CollectionOfWorkers.PreviewName))?.FileName ?? "");

                SaveImagesInDB(userImage);

                userImage.ResizedImages = new List<ResizedImageWithPreview>();

                ResizedImagesList.Where(f => f.FileSourceName == userImage.FileName && f.FileName != userImage.PreviewPath).
                ToList().ForEach(imageInfo => SaveResizedImagesInDB(userImage, imageInfo));
            });
        }

        private void SaveResizedImagesInDB(Image userImage, ResizedImageInfo imageInfo)
        {
            using (var db = new ImageContext())
            {

                var ResizedImage = new ResizedImage()
                {
                    ParentId = userImage.Id,
                    FileName = imageInfo.FileName,
                    FilePath = Url.Content("~/Images/" + imageInfo.FileName),
                    PreviewPath = userImage.PreviewPath,
                    Height = imageInfo.Height,
                    Width = imageInfo.Width,
                    StartTime = imageInfo.StartTime,
                    FinishTime = imageInfo.FinishTime
                };

                try
                {
                    db.ResizedImages.Add(ResizedImage);
                    db.SaveChanges();
                    userImage.ResizedImages.Add(new ResizedImageWithPreview(ResizedImage));
                }
                catch (Exception ex)
                {

                }
            }
        }

        private List<Image> GetImagesFromRequest(ref List<string> ImagesForWorker)
        {
            var userImages = new List<Image>();

            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                var userImage = new Image();
                if (upload != null)
                {
                    // получаем имя файла
                    string fileName = Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    userImage.FilePath = Url.Content("~/Images/" + fileName);
                    userImage.FileName = fileName;
                    upload.SaveAs(Server.MapPath(userImage.FilePath));
                    using (var img = System.Drawing.Image.FromFile(Server.MapPath(userImage.FilePath)))
                    {
                        userImage.Height = img.Height;
                        userImage.Width = img.Width;
                    }
                    ImagesForWorker.Add(Server.MapPath(userImage.FilePath));
                }
                userImages.Add(userImage);
            }
            return userImages;
        }

        [HttpPost]
        public JsonResult AddSetting(ResizeSettingsModel model)
        {
            var uuid = Request["uuid"];
            if (!string.IsNullOrEmpty(uuid))
            {
                CollectionOfWorkers.GetWorker(uuid).Settings.Add(
                    new ResizeSettingsModel() { Name = model.Name, Height = model.Height, Width = model.Width }
                   );
            }
            else
            {
                throw new Exception("uuid is not passed");
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult SetThreadCount(int Count)
        {
            //todo 
            // set threadcount
            var uuid = Request["uuid"];
            if (!string.IsNullOrEmpty(uuid))
            {
                CollectionOfWorkers.GetWorker(uuid).MaxImagesCountinOneThread = Count;
            }
            else
            {
                throw new Exception("uuid is not passed");
            }
            return Json(Count);
        }

        public HttpResponseMessage GetPreview(string filePath)
        {
            // resize

            var fileStream = new FileStream(Server.MapPath(filePath), FileMode.Open);
            var resp = new HttpResponseMessage()
            {
                Content = new StreamContent(fileStream)
            };
            fileStream.Dispose();
            // Find the MIME type
            string mimeType = Path.GetExtension(filePath).Replace(".", "image/");
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            return resp;
        }


    }
}