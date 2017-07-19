using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
//using System.ComponentModel.DataAnnotations;


namespace ABCAPOS.Models
{
    public class AssemblyBuildDetailModel
    {
        public long AssemblyBuildID { get; set; }
        public long WorkOrderID { get; set; }
        public long ProductDetailID { get; set; }
        public int ItemNo { get; set; }
        public long TypeID { get; set; }
        public long WarehouseID { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
   
        public double Qty { get; set; }
        public int TaxType
        {
            get
            {
                return 1;
            }
        }
        //public long UnitID { get; set; }
        public double Price { get; set; }
        public double amount { get; set; }
        public double TaxAmount { get; set; }
        public long ProductID { get; set; }
        public string WorkOrderCode { get; set; }
        public double QtyWO { get; set; }
        //public long QtyBuild { get; set; }

        public string ProductDetailCode { get; set; }
        public string ProductDetailName { get; set; }
        public string ConversionName { get; set; }
        public string WarehouseName { get; set; }
        public string DepartmentName { get; set; }
        public long ConversionID { get; set; }
        public long ConversionIDTemp { get; set; }
        public double UnitRate { get; set; }
        public double Total { get; set; }
        //public long WorkOrderID { get; set; }
        //public List<AssemblyBuildDetailModel> details { get; set; }

        public long StockID { get; set; }

        public string TaxTypeName
        {
            get
            {
                if (TaxType == 1)
                    return "Non-PPN";
                else
                    return "PPN";
            }
        }

        public AssemblyBuildDetailModel()
        {
            //details = new List<AssemblyBuildDetailModel>();
        }

    }
}
