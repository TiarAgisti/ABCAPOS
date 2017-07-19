using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PurchaseOrderDetailModel
    {
        public long PurchaseOrderID { get; set; }
        public int ItemNo { get; set; }
        public int BookingOrderItemNo { get; set; }
        public DateTime Date { get; set; }
        public string PurchaseOrderCode { get; set; }
        public int LineSequenceNumber { get; set; }
        public long ProductID { get; set; }
        public decimal Price { get; set; }
        public decimal AssetPrice { get; set; }
        public decimal AssetPriceInDollar { get; set; }
        public double Quantity { get; set; }
        public double Discount { get; set; }
        public double BatchNo { get; set; }

        public double QtyRemaining
        {
            get;
            set;
        }

        // should be ReadOnly
        public double CreatedBOQuantity
        {
            get;
            set;
        }
        // should be ReadOnly
        public double CreatedPDQuantity
        {
            get; set;
        }
        // should be ReadOnly
        public double CreatedPBQuantity
        {
            get; set; 
        }

        public double CreatedReturnQuantity { get; set; }

        public string Remarks { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string VendorName { get; set; }

        public int TaxType { get; set; }
        public string TaxRate { get; set; }
        public double UnitRate { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public long ConversionIDTemp { get; set; }
        public double StockQty { get; set; }
        public double StockQtyHidden { get; set; }
        public double StockAvailable { get; set; }
        public double StockAvailableHidden { get; set; }

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

        public decimal TotalAmount
        {
            get
            {
                return (decimal)AssetPrice * (decimal)Quantity;
            }
        }

        public decimal TotalPPN
        {
            get
            {
                if (TaxType == 2)
                    return ((decimal)TotalAmount - (decimal)Discount) * (decimal)0.1;
                else
                    return 0;
            }
        }

        public decimal Total
        {
            get;
            set;
        }

        public double OutstandingQuantity
        {
            get
            {
                return Convert.ToDouble(Quantity - CreatedPDQuantity);
            }
        }

        public double OutstandingPBQuantity
        {
            get
            {
                return Convert.ToDouble(Quantity - CreatedPBQuantity);
            }
        }

        public decimal PriceAfterDiscount
        {
            get
            {
                return (Convert.ToDecimal(AssetPriceInDollar) - Convert.ToDecimal(Discount));
            }
        }

        public decimal TotalRupiah
        {
            get
            {
                return (Convert.ToDecimal(AssetPriceInDollar) - Convert.ToDecimal(Discount))* Convert.ToDecimal(Quantity);
            }
        }

        /* Read Only for v_RelatedPOandPurchaseBill  && v_RelatedBuildByPO*/
        public long ID { get; set; }
        public long PurchaseBillID { get; set; }
        public int PurchaseOrderItemNo { get; set; }
        public double QuantityPurchaseBill { get; set; }
        public double QuantityPurchaseOrder { get; set; }
        public long AssemblyBuildID { get; set; }
        public long LogID { get; set; }
        public double MovingOutQty { get; set; }
        public long ContainerID { get; set; }
        public double ContainerPrice { get; set; }
        public decimal TotalPODetail { get; set; }
        public int TaxTypePO { get; set; }
        public double TotalAmountPBDetail { get; set; }
        public double TotalPPNPBDetail { get; set; }
        public double TotalPBDetail { get; set; }
        /* Read Only for v_RelatedPOandPurchaseBill */

    }
}
