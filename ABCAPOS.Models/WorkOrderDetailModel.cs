using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class WorkOrderDetailModel
    {
        public DateTime Date { get; set; }
        public long WorkOrderID { get; set; }
        public int ItemNo { get; set; }
        public long ProductID { get; set; }
        public long ProductDetailID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public double QtyOnHand { get; set; }
        public double StockQtyHidden { get; set; }
        public double QtyAvailable { get; set; }
        public double StockAvailableHidden { get; set; }

        [DisplayFormat(DataFormatString = "{2:C}", ApplyFormatInEditMode = true)]
        public double Qty { get; set; }
        public double UsedInBuilt { get; set; }
        public long UnitID { get; set; }
        public string ConversionName { get; set; }
        public long ConversionID { get; set; }
        public long ConversionIDTemp { get; set; }
        public int ItemTypeID { get; set; }
        public string ItemType { get; set; }
        public string SpesialOrder { get; set; }
        public double UnitRate { get; set; }
        public string StrQuantity { get; set; }
        public int Status { get; set; }
        public string BatchNo { get; set; }
        public List<WorkOrderDetailModel> Details { get; set; }

        public WorkOrderDetailModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<WorkOrderDetailModel>();
        }
    }
}
