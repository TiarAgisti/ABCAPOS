using ABCAPOS.BF;
using ABCAPOS.Models;
using MPL.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS.Controllers
{
    public class StockMovementController : Controller
    {
        public ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            var stockList = new List<StockMovementModel>();
            var stockCount = 0;

            if (filter.FilterFields.Count > 0)
            {
                var code = filter.FilterFields[0].Value;
                var type = filter.FilterFields[1].Value;
                var startdate = Convert.ToDateTime(filter.FilterFields[2].Value);
                var endDate = Convert.ToDateTime(filter.FilterFields[2].Value1);
                var product = new ProductBFC().RetrieveByCode(code);

                if (product != null)
                {
                    stockList = new LogBFC().RetrieveStockMovement(product.ID, Convert.ToInt32(type), Convert.ToDateTime(startdate), Convert.ToDateTime(endDate), (int)startIndex, (int)amount);

                    stockCount = new LogBFC().RetrieveStockMovementCount(product.ID, Convert.ToInt32(type), Convert.ToDateTime(startdate), Convert.ToDateTime(endDate));
                }
            }
            
            ViewBag.DataCount = stockCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            //ViewBag.PageSeriesSize = (poCount/amount);

            return View(stockList);
        }
    }
}
