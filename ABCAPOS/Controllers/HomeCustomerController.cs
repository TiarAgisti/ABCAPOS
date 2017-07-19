using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Models;

namespace ABCAPOS.Controllers
{
    public class HomeCustomerController : Controller
    {
        

        public ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.Year = DateTime.Now.Year.ToString();
            
            //Qty Sold
            List<ProductModel> productSold = new ProductBFC().RetriveSalesOrderItemSoldList(filter.GetSelectFilters(), DateTime.Now.Year);
            ViewBag.SalesOrderItemSold = productSold;

            return View(productSold);
        }


    }
}
