using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ResizedImagesController : ApiController
    {
        private ImageContext db = new ImageContext();

        // GET: api/ResizedImage/1/namedesc
        public ImagesByPage GetResizedImage(int page, string sort)
        {
            return new ImagesByPage(db.ResizedImages.Count(), GetImagesByPage(page, sort).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private IQueryable<ResizedImage> GetImagesByPage(int page, string sort)
        {
            var result = db.ResizedImages.OrderBy(i => i.Id);
            switch (sort)
            {
                case "idasc":
                    result = db.ResizedImages.OrderBy(i => i.Id);
                    break;
                case "iddesc":
                    result = db.ResizedImages.OrderByDescending(i => i.Id);
                    break;
                case "nameasc":
                    result = db.ResizedImages.OrderBy(i => i.FileName);
                    break;
                case "namedesc":
                    result = db.ResizedImages.OrderByDescending(i => i.FileName);
                    break;
                case "parentasc":
                    result = db.ResizedImages.OrderBy(i => i.ParentId);
                    break;
                case "parentdesc":
                    result = db.ResizedImages.OrderByDescending(i => i.ParentId);
                    break;
                case "startasc":
                    result = db.ResizedImages.OrderBy(i => i.StartTime);
                    break;
                case "startdesc":
                    result = db.ResizedImages.OrderByDescending(i => i.StartTime);
                    break;
                case "finishasc":
                    result = db.ResizedImages.OrderBy(i => i.FinishTime);
                    break;
                case "finishdesc":
                    result = db.ResizedImages.OrderByDescending(i => i.FinishTime);
                    break;
                case "widthtasc":
                    result = db.ResizedImages.OrderBy(i => i.Width);
                    break;
                case "widthdesc":
                    result = db.ResizedImages.OrderByDescending(i => i.Width);
                    break;
                case "heightasc":
                    result = db.ResizedImages.OrderBy(i => i.Height);
                    break;
                case "heightdesc":
                    result = db.ResizedImages.OrderByDescending(i => i.Height);
                    break;
                default:
                    break;
            }

            return result.Skip(page < 2 ? 0 : CollectionOfWorkers.SizeOfPage * (page - 1)).
                Take(CollectionOfWorkers.SizeOfPage);
        }
        
    }
}