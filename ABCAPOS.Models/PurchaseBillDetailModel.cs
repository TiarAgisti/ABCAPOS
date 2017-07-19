using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PurchaseBillDetailModel
    {
        public long PurchaseBillID { get; set; }
        public int ItemNo { get; set; }
        public int PurchaseOrderItemNo { get; set; }
        public DateTime Date { get; set; }
        public string PurchaseBillCode { get; set; }
        public string VendorName { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public decimal AssetPrice { get; set; }
        public double Discount { get; set; }
        public string Remarks { get; set; }

        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }
        public int TaxType { get; set; }

        public string StrQuantity { get; set; }
        public string StrTotal { get; set; }
        public double QtyPO { get; set; }
        public double QtyRemain { get; set; }
        public double StockQty { get; set; }
        public double QtyReceive { get; set; }
        // ReadOnly (via View)
        public double CreatedPOQuantity
        {
            get;
            set;
        }
        public double CreatedPDQuantity
        {
            get;
            set;
        }
        public double CreatedPBQuantity { get; set; }
        public double SelisihQuantity { get; set; }
        public double TaxAmount { get; set; }

        public decimal TotalAmount
        {
            get
            {
                return Convert.ToDecimal(AssetPrice) * Convert.ToDecimal(Quantity);
            }
        }

        public decimal TotalPPN
        {
            get
            {
                if (TaxType == 2)
                    return (TotalAmount - Convert.ToDecimal(Discount)) * Convert.ToDecimal(0.1);
                else
                    return 0;
            }
        }

        public decimal Total{get;set;}

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

        public double OutstandingPBQuantity
        {
            get
            {
                return Convert.ToDouble(CreatedPDQuantity - CreatedPBQuantity);
            }
        }
    }
}
