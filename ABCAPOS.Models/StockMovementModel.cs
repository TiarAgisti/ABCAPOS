using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class StockMovementModel
    {
        public long LogID { get; set; }
        public DateTime Date { get; set; }
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public double BeginningQty { get; set; }
        public double MovingInQty { get; set; }
        public double MovingOutQty { get; set; }
        public double EndingQty { get; set; }
        public long TransactionID { get; set; }
        public string TransactionCode { get; set; }
        public int Type { get; set; }

        public string DocumentType
        {
            get
            {
                if (Type == 1)
                    return "Item Receipt";
                else if (Type == 2)
                    return "Item Fulfillment";
                else if (Type == 3)
                    return "Assembly Build";
                else if (Type == 4)
                    return "Inventory Adjustment";
                else if (Type == 5)
                    return "Cash Sales";
                else if (Type == 6)
                    return "Return Receipt";
                else if (Type == 7)
                    return "Transfer Delivery";
                else if (Type == 8)
                    return "Transfer Receipt";
                else if (Type == 9)
                    return "Vendor Return Delivery";
                else if (Type == 10)
                    return "Assembly Unbuild";

                return "";
            }
        }

        public string Url
        {
            get
            {
                if (Type == 1)
                    return "PurchaseDelivery/Detail?key=" + TransactionID;
                else if (Type == 2)
                    return "DeliveryOrder/Detail?key=" + TransactionID;
                else if (Type == 3)
                    return "AssemblyBuild/Detail?key=" + TransactionID;
                else if (Type == 4)
                    return "InventoryAdjustment/Detail?key=" + TransactionID;
                else if (Type == 5)
                    return "CashSales/Detail?key=" + TransactionID;
                else if (Type == 6)
                    return "ReturnReceipt/Detail?key=" + TransactionID;
                else if (Type == 7)
                    return "TransferDelivery/Detail?key=" + TransactionID;
                else if (Type == 8)
                    return "TransferReceipt/Detail?key=" + TransactionID;
                else if (Type == 9)
                    return "VendorReturnDelivery/Detail?key=" + TransactionID;
                else if (Type == 10)
                    return "AssemblyUnBuild/Detail?key=" + TransactionID;

                return "";
            }
        }
    }
}
