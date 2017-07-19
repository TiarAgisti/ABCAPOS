using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceDetailModel
    {
        public long InvoiceID { get; set; }
        public int ItemNo { get; set; }
        public int SalesOrderItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public string Remarks { get; set; }

        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }
        public int TaxType { get; set; }

        public string StrQuantity { get; set; }
        public double QtySO { get; set; }
        public double QtyRemain { get; set; }
        public double StockQty { get; set; }
        public double QtyFulfill { get; set; }
        public long PriceLevelID { get; set; }
        public string PriceLevelName { get; set; }
        public double CreatedSOQuantity { get; set; }
        public double CreatedDOQuantity { get; set; }
        public double CreatedInvQuantity { get; set; }
        public double SelisihQuantity { get; set; }
        public decimal GrossAmount { get; set; }
        public bool HasInvoiceItem { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceCode { get; set; }
        public string CustomerName { get; set; }

        public decimal TotalAmount {get; set; }
        public long ID { get; set; }
        //{
        //    get
        //    {
        //        return Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(Price) * Convert.ToDecimal(Quantity)).ToString("N0"));
        //    }
        //    set
        //    {
        //    }
        //}

        public decimal TotalPPN {get; set; }
        //{
        //    get
        //    {
        //        if (TaxType == 2)
        //            return TotalAmount * Convert.ToDecimal(0.1);
        //        else
        //            return 0;
        //    }
        //    set
        //    {
        //    }
        //}

        public decimal Total
        {
            get
            {
                return TotalAmount + TotalPPN;
            }
        }

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

        public double OutstandingInvQuantity
        {
            get
            {
                return Convert.ToDouble(CreatedDOQuantity - CreatedInvQuantity);
            }
        }
    }
}
