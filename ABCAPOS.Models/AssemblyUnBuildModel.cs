using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class AssemblyUnBuildModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public long ProductID { get; set; }
        [Required]
        public decimal QtyUnBuild { get; set; }
        public decimal QtyActual { get; set; }
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
        public int Status { get; set; }
        public string VoidRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string Notes { get; set; }
        public long LogID { get; set; }
        public string Viskositas { get; set; }
        public string BeratJenis { get; set; }
        public string WaktuMulaiProduksi { get; set; }
        public string WaktuSelesaiProduksi { get; set; }
        public List<AssemblyUnBuildDetailModel> Details { get; set; }
        

        //Read Only
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Class { get; set; }
        public string ItemBrand { get; set; }
        public string ItemProduct { get; set; }
        public string UnitName { get; set; }

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

        public string StatusDescription { get; set; }
        public string GroupWarna { get; set; }
        public string ItemTyoe { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
        public int ItemTypeID { get; set; }
        public long StockUnitID { get; set; }

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

        public AssemblyUnBuildModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<AssemblyUnBuildDetailModel>();
        }

    }
}
