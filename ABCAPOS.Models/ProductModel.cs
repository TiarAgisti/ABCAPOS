using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class ProductModel
    {
        public long ID { get; set; }

        [Required]
        public string Code { get; set; }

        public string ItemProduct { get; set; }
        public string ItemBrand { get; set; }
        public string Type { get; set; }
        public string GroupWarna { get; set; }
        [Required]
        public string ProductName { get; set; }
        public decimal AssetPrice { get; set; }
        [Required]
        public string Category { get; set; }
        public string Class { get; set; }
        public double StockAvailable { get; set; }
        public double StockQty { get; set; }
        [Required]
        public string Barcode { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal SellingPrice { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsPPNIncluded { get; set; }
        [Required]
        public long CategoryID { get; set; }
        public string Packaging { get; set; }
        public string Specification { get; set; }
        public long VendorID { get; set; }
        public int ItemTypeID { get; set; }
        public string ItemTypeDesc { get; set; }

        public bool UseBin { get; set; }
        
        public string VendorName { get; set; }

        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        
        public decimal AssetPriceInDollar { get; set; }
        public long No { get; set; }
        public double QtySold { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        public string ProductCode { get; set; }
        public long CustomerID { get; set; }

        public decimal Discount1
        {
            get
            {
                return BasePrice * (decimal)0.8123;
            }
        }
        public decimal Discount2
        {
            get
            {
                return BasePrice * (decimal)0.8550;
            }
        }
        public decimal Discount3
        {
            get
            {
                return BasePrice * (decimal)0.9000;
            }
        }
        public decimal Discount4
        {
            get
            {
                return BasePrice * (decimal)0.8370;
            }
        }
        public decimal Discount5
        {
            get
            {
                return BasePrice * (decimal)0.8280;
            }
        }
        public decimal Discount6
        {
            get
            {
                return BasePrice * (decimal)0.8000;
            }
        }

        [Required]
        public long UnitTypeID { get; set; }
        public string UnitTypeName { get; set; }
        [Required]
        public long StockUnitID { get; set; }
        public string StockUnitName { get; set; }
        public long PurchaseUnitID { get; set; }
        public string PurchaseUnitName { get; set; }
        public long SaleUnitID { get; set; }
        public string SaleUnitName{ get; set; }
        public List<ProductDetailModel> Details { get; set; }

        public List<ItemLocationModel> ItemLocations { get; set; }
        public List<BasePriceLocationModel> basePriceDetails { get; set; }
        public List<LimitStockModel> LimitStockDetails { get; set; } /*penambahan by tiar*/
        public List<FormulasiModel> FormulasiDetails { get; set; } /*penambahan by tiar */
        //public List<WorkOrderModel> WorkOrderDetails { get; set; } /*penambahan by tiar */

        public string CategoryDescription
        {   
            get
            {
                if (CategoryID == (int)ProductCategory.Product)
                    return "Produk";
                else if (CategoryID == (int)ProductCategory.Service)
                    return "Service";
                else if (CategoryID == (int)ProductCategory.NonStock)
                    return "Non-Stock";
                else
                    return "";
            }
        }

        public string IsActiveDescription
        {
            get
            {
                if (IsActive)
                    return "Aktif";
                else
                    return "Tidak Aktif";
            }
        }
       

        public ProductModel()
        {
            IsActive = true;
        }

        #region ListPendingWO
        public long ProductID { get; set; }
        public long ItemLocationID { get; set; }
        public long LocationID { get; set; }
        public string LocationName { get; set; }
        public string UnitName { get; set; }
        public Double Qty_Minimum { get; set; }
        public Double QtyOnHand { get; set; }
        public Double Presentase { get; set; }

        public string PercentageText
        {
            get
            {
                this.Presentase = Math.Round(this.Presentase, 2);
                return this.Presentase + "%";
            }
        }
        #endregion
    }
}
