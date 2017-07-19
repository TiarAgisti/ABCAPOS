using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class AssemblyBuildModel
    {
        
        public long ID { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public long WorkOrderID { get; set; }
        public string WorkOrderCode { get; set; }
        public long ProductID { get; set; }
        public long ProductDetailID { get; set; }
        public decimal QtyWO { get; set; }
        [Required]
        public decimal QtyBuild { get; set; }
        public decimal QtyActual { get; set; }
        public decimal QtyLost { get; set; }
        public long UnitID { get; set; }
        public Decimal TotalProject { get; set; }
        public decimal Hpp { get; set; }
        public DateTime Tanggal{ get; set; }
        public long WarehouseID
        {
            get
            {
                return 2;
            }
        }

        public long DepartmentID
        {
            get
            {
                return 4;
            }
        }

        public string VoidRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ProductCode { get; set; }
        public string ProductDetailCode { get; set; }
        public string ProductName { get; set; }
        public string ProductDetailName { get; set; }
        public string Class { get; set; }
        public string ItemBrand { get; set; }
        public string ItemProduct { get; set; }
        public string UnitName { get; set; }
        public string BatchNo { get; set; }

        public string WarehouseName
        {
            get
            {
                return "Factory";
            }
        }

        public string DepartmentName
        {
            get
            {
                return "Operation : Manufacturing";
            }
        }

        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public string GroupWarna { get; set; }
        public string ItemType { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
        public int TaxType { get; set; }
        public decimal GrandTotal { get; set; }
        public string Viskositas { get; set; }
        public string BeratJenis { get; set; }
        public string WaktuMulaiProduksi { get; set; }
        public string WaktuSelesaiProduksi { get; set; }
        public int ItemTypeID { get; set; }
        public long StockUnitID { get; set; }

        public string Notes { get; set; }
        public string WoNotes { get; set; }

        public List<AssemblyBuildDetailModel> Details { get; set; }

        public long LogID { get; set; }

        public AssemblyBuildModel()
        {
            Date = DateTime.Now;
            Tanggal = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<AssemblyBuildDetailModel>();
        }

        public string TaxTypeName
        {
            get
            {
                if (TaxType == (int)Util.TaxType.NonTax)
                    return "Non Tax";
                else if (TaxType == (int)Util.TaxType.PPN)
                    return "PPN";
                else
                    return "";
            }
        }

        public string StatusCreated
        {
            get
            {
                return CreatedBy + " - on :" + CreatedDate;
            }
        }

        public string StatusModified
        {
            get
            {
                return ModifiedBy + " - on :" + ModifiedDate;
            }
        }


    }
}
