using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
namespace Maison.Controllers
{
    public class HomeController : Controller
    {
        shopdb db=new shopdb();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult dropdanhmuc()
        {
            IEnumerable<Danhmuc> danhmucs = db.Danhmucs.Select(p => p);
            return PartialView(danhmucs);
        }



    }
}