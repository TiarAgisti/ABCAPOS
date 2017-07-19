using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Models;

namespace ABCAPOS.Controllers
{
    public class HomeProductController : Controller
    {
        //
        // GET: /HomeProduct/

        public ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.Year = DateTime.Now.Year.ToString();

            //Qty Sold
            List<ProductModel> productSold = new ProductBFC().RetriveProductItemSoldList(filter.GetSelectFilters(), DateTime.Now.Year);
            ViewBag.ProductItemSold = productSold;

            return View(productSold);
        }

    }
}
