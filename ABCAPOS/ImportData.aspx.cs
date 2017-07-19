using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using ABCAPOS.DA;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.POIFS.FileSystem;
using NPOI.HPSF;
using System.EnterpriseServices;
using System.Configuration;
using System.Transactions;

namespace ABCAPOS
{
    public partial class ImportData : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                this.PostingUlangBuild();
        }

        private void PostingUlangInvoicePayment()
        {
            try
            {
                //Get machineSettings session
                var machineSettings = (System.Transactions.Configuration.MachineSettingsSection)ConfigurationManager.GetSection("system.transactions/machineSettings");
                //Allow modifications
                var bReadOnly = (typeof(ConfigurationElement)).GetField("_bReadOnly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bReadOnly.SetValue(machineSettings, false);
                //Change max allowed timeout
                machineSettings.MaxTimeout = TimeSpan.MaxValue;

                var details = new MakeMultiPaySalesBFC().RetreiveDataPostingUlangInvoicePayment();

                foreach (var detail in details)
                {
                    var header = new MakeMultiPaySalesBFC().RetrieveByID(detail.ID);

                    new MakeMultiPaySalesBFC().PostAccounting(header.ID,header.Status);
                }

                LblSukses.Text = "Posting Ulang Berhasil";

                //using (TransactionScope trans = new TransactionScope())
                //{
                   

                //    trans.Complete();
                //}
               
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void PostingUlangBuild()
        {
            try
            {
                //Get machineSettings session
                var machineSettings = (System.Transactions.Configuration.MachineSettingsSection)ConfigurationManager.GetSection("system.transactions/machineSettings");
                //Allow modifications
                var bReadOnly = (typeof(ConfigurationElement)).GetField("_bReadOnly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bReadOnly.SetValue(machineSettings, false);
                //Change max allowed timeout
                machineSettings.MaxTimeout = TimeSpan.MaxValue;

                var details = new AssemblyBuildBFC().RetrieveAll().Where(e => e.Status == 3);
                //var invoiceno = 02097;

                foreach (var detail in details)
                {
                    //detail.invoiceno = invoiceno + 1;


                    new AssemblyBuildBFC().PostAccounting(detail.ID, detail.Status);
                }

                LblSukses.Text = "Posting Ulang Berhasil";
            }
            catch (Exception ex)
            {
                LblSukses.Text = "Posting Ulang EROR";
            }
        }
        //private void Import()
        //{
        //   ImportSalesOrderDetail();
        //    //importProduct();
        //    //importMarketing();
        //   //importVendor();
        //   //importStaff();
        //    //ImportDiamond("FancyColor", "13");
        //    //ImportWatch();
        //    //ImportBracelet();
        //    //ImportSettingBracelet();
        //    //ImportBrooch();
        //    //ImportSettingBrooch();
        //    //ImportEarring();
        //    //ImportSettingEarring();
        //    //ImportNecklace();
        //    //ImportSettingNecklace();
        //    //ImportRing();
        //    //ImportSettingRing();
        //    //ImportRapaport("Round");
        //    // ImportRapaport("Pear");
        //}

        //private void importProduct()
        //{
        //    var filename = "SKP WASUKA, form loading, kartu stok (2) buat di cetak";
        //    if (File.Exists(Server.MapPath("~/Import/SKP WASUKA, form loading, kartu stok (2) buat di cetak.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + filename + ".xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            if (String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(0))) && String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(1))))
        //                break;

        //            var product = new ProductModel();

        //            product.Code = new ProductBFC().GetProductCode();
        //            product.Barcode = getValueFromCell(sheet.GetRow(row).GetCell(1));
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(2));
        //            product.BaseDoosPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(3)));
        //            //product.StockQty = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(3)));
        //            //product.StockSo  = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(5)));
        //           // product.QtyInDoos = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.Specification = "";
                    
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate  = DateTime.Now.Date;
        //            //product.MAP = false;
                    
        //            //customer.EffectiveEndDate = DateTime.MaxValue.Date;
        //            //product.City = "";
                    
        //            //customer.EffectiveStartDate = DateTime.MinValue;
        //            //product.Fax = "";
                    
        //            new ProductBFC().Create(product);
        //        }
        //  }
        
        //}
        //private void importVendor()
        //{
        //    var filename = "vendor";
        //    if (File.Exists(Server.MapPath("~/Import/vendor.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + filename + ".xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            if (String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(0))) && String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(1))))
        //                break;

        //            var vendor = new VendorModel();

        //            vendor.Code = new VendorBFC().GetVendorCode();
        //            vendor.Name = getValueFromCell(sheet.GetRow(row).GetCell(1));
        //            vendor.Address = getValueFromCell(sheet.GetRow(row).GetCell(2));
        //            vendor.Phone= getValueFromCell(sheet.GetRow(row).GetCell(3));
        //            //vendor.StockSo = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //            //vendor.Specification = "";

        //            vendor.IsActive = true;
        //            vendor.CreatedBy = vendor.ModifiedBy = "SYSTEM";
        //            vendor.CreatedDate = vendor.ModifiedDate = vendor.EffectiveStartDate = vendor.EffectiveEndDate = DateTime.Now.Date;
        //            //product.MAP = false;

        //            //customer.EffectiveEndDate = DateTime.MaxValue.Date;
        //            //product.City = "";

        //            //customer.EffectiveStartDate = DateTime.MinValue;
        //            //product.Fax = "";

        //            new VendorBFC().Create(vendor);
        //        }
        //    }

        //}
        //private void importMarketing()
        //{
        //    var filename = "marketing";
        //    if (File.Exists(Server.MapPath("~/Import/marketing.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + filename + ".xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            if (String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(0))) && String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(1))))
        //                break;

        //            var salesman = new SalesmanModel();

        //            salesman.Code = new SalesmanBFC().GetSalesmanCode();
        //            salesman.Name = getValueFromCell(sheet.GetRow(row).GetCell(1));
        //            salesman.Address = getValueFromCell(sheet.GetRow(row).GetCell(2));
        //            salesman.Phone1 = getValueFromCell(sheet.GetRow(row).GetCell(4));
        //           // salesman.Specification = "";

        //            salesman.IsActive = true;
        //            salesman.CreatedBy = salesman.ModifiedBy = "SYSTEM";
        //            salesman.CreatedDate = salesman.ModifiedDate = salesman.EffectiveStartDate = salesman.EffectiveEndDate = DateTime.Now.Date;
        //            //product.MAP = false;

        //            //customer.EffectiveEndDate = DateTime.MaxValue.Date;
        //            //product.City = "";

        //            //customer.EffectiveStartDate = DateTime.MinValue;
        //            //product.Fax = "";

        //            new SalesmanBFC().Create(salesman);
        //        }
        //    }

        //}
        //private void importStaff()
        //{
        //    var filename = "staff";
        //    if (File.Exists(Server.MapPath("~/Import/staff.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + filename + ".xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            if (String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(0))) && String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(1))))
        //                break;

        //            var staff = new StaffModel();

        //            //staff.Code = new StaffBFC().Get();
        //            staff.Name = getValueFromCell(sheet.GetRow(row).GetCell(1));
        //            staff.BasicSalary = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(2)));
        //           // staff.StockSo = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //            //staff.Specification = "";

        //            staff.IsActive = true;
        //            staff.CreatedBy = staff.ModifiedBy = "SYSTEM";
        //            staff.CreatedDate = staff.ModifiedDate = DateTime.Now.Date;
        //            //product.MAP = false;

        //            //customer.EffectiveEndDate = DateTime.MaxValue.Date;
        //            //product.City = "";

        //            //customer.EffectiveStartDate = DateTime.MinValue;
        //            //product.Fax = "";

        //            new StaffBFC().Create(staff);
        //        }
        //    }

        //}
        //private void ImportRapaport(string rapType)
        //{
        //    string[] filePaths = Directory.GetFiles(Server.MapPath("~/Import"), "*.xls");

        //    foreach (string filePath in filePaths)
        //    {

        //        var fileName = Path.GetFileName(filePath);
        //        if (fileName.Contains(rapType))
        //        {
        //            var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + fileName)));
        //            ISheet sheet = workbook.GetSheetAt(0);
        //            string[] rapaportDate = getValueFromCell(sheet.GetRow(0).GetCell(6)).Split('/');

        //            int caratID = 0;
        //            int caratNewID = 0;
        //            long rapaportID = 0;
        //            int itemNo = 1;

        //            for (int row = 0; row <= sheet.LastRowNum; row++)
        //            {
        //                var clarity = getValueFromCell(sheet.GetRow(row).GetCell(1));
        //                var color = getValueFromCell(sheet.GetRow(row).GetCell(2));
        //                var minimumCarat = getValueFromCell(sheet.GetRow(row).GetCell(3));
        //                var marketPrice = getValueFromCell(sheet.GetRow(row).GetCell(5));
        //                caratID = new IDNumericSayer().getCaratID(Convert.ToDouble(minimumCarat));

        //                if (caratID != caratNewID)
        //                {
        //                    RapaportModel rapaport = new RapaportModel();
        //                    rapaport.Code = new RapaportBFC().GetRapaportCode();
        //                    rapaport.Date = new DateTime(Convert.ToInt32(rapaportDate[2]), Convert.ToInt32(rapaportDate[0].Trim()), Convert.ToInt32(rapaportDate[1]));
        //                    rapaport.CaratID = caratID;
        //                    if (rapType == "Round")
        //                        rapaport.Category = "ROUNDS";
        //                    else if (rapType == "Pear")
        //                        rapaport.Category = "PEARS";

        //                    rapaport.CreatedBy = rapaport.ModifiedBy = "SYSTEM";
        //                    rapaport.CreatedDate = rapaport.ModifiedDate = DateTime.Now;

        //                    new RapaportBFC().Create(rapaport);
        //                    //hard reset
        //                    rapaportID = rapaport.ID;
        //                    caratNewID = caratID;
        //                    itemNo = 1;
        //                }
        //                var rapaportDetail = new RapaportDetailModel();
        //                rapaportDetail.RapaportID = rapaportID;
        //                rapaportDetail.CaratID = caratID;
        //                var xID = new MDPOSDAC().RetrieveDiamondTypeSpecificationID(clarity, "clarity");
        //                var yID = new MDPOSDAC().RetrieveDiamondTypeSpecificationID(color, "color");

        //                rapaportDetail.DiamondSpecificationID = new MDPOSDAC().RetrieveDiamondSpecIDByCaratIDSpecXSpecY(caratID, xID, yID);
        //                rapaportDetail.MarketPrice = Convert.ToDecimal(marketPrice);
        //                rapaportDetail.ItemNo = itemNo;
        //                new RapaportDetailBFC().Create(rapaportDetail);

        //                itemNo++;
        //            }

        //        }
        //    }

        //}

        //private void ImportWatch()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Watch.xls")))
        //    {

        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Watch.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var customer= new Customer();
        //            string typeID = "2";

        //            customer.Code = new CustomerBFC().GetCustomerCode();
        //            customer.Name = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            //customer.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(13)));
        //            customer.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(14)));
        //            customer.Description = getValueFromCell(sheet.GetRow(row).GetCell(15));
        //            customer.Type = typeID;
        //            customer.IsActive = true;
        //            customer.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            customer.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new CustomertBFC().Create(customer);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(16));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(17)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 12; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //    }
        //    //File.Delete(Server.MapPath("~/Import/Diamond.xls"));
        //}

        //public void CreateSODetails(long soID, List<SalesOrderDetailModel> soDetails)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();

        //    foreach (var soDetail in soDetails)
        //    {
        //        soDetail.SalesOrderID = soID;
        //        var itemNo = soDetail.ItemNo;
        //        soDetail.ItemNo = itemNo;

        //        SalesOrderDetail obj = new SalesOrderDetail();
        //        ObjectHelper.CopyProperties(soDetail, obj);
        //        ent.AddToSalesOrderDetail(obj);
        //    }

        //    ent.SaveChanges();
        //}

        //private void ImportSalesOrderDetail()
        //{
        //  var filename = "BU Sales Order Detail";
        //  if (File.Exists(Server.MapPath("~/Import/BU Sales Order Detail.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/" + filename + ".xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        var SONumber = "";
        //        var SODItemNo = 1;
        //        for (int row = 2; row <= sheet.LastRowNum; row++)
        //        {
        //            //if (String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(2))) && String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(3))))
        //            //    break;
        //            if (!String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(0))))
        //            {
        //                SONumber = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //                SODItemNo = 1;
        //            }
        //            var salesOrder = new SalesOrderBFC().RetrieveByCode(SONumber);
        //            if (salesOrder != null)
        //            {
        //                if (!String.IsNullOrEmpty(getValueFromCell(sheet.GetRow(row).GetCell(4))))
        //                {
        //                    var soDetails = new List<SalesOrderDetailModel>();
        //                    var soDetail = new SalesOrderDetailModel();
        //                    var product = new ProductBFC().RetrieveByCode(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //                    if (product != null)
        //                    {
        //                        soDetail.ItemNo = SODItemNo;
        //                        soDetail.ProductID = product.ID;
        //                        soDetail.ConversionID = (new UnitBFC().GetUnitDetailByUnitIDAndPluralAbbreviation(product.UnitTypeID, getValueFromCell(sheet.GetRow(row).GetCell(11)))).ID;
        //                        soDetail.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(6)));
        //                        soDetail.QtyPicked = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //                        soDetail.QtyPacked = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(8)));
        //                        soDetail.QtyShipped = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(9)));
        //                        if (getValueFromCell(sheet.GetRow(row).GetCell(13)).ToString() == "0.1")
        //                            soDetail.TaxType = (int)TaxType.PPN;
        //                        else
        //                            soDetail.TaxType = (int)TaxType.NonTax;

        //                        soDetail.PriceLevelID = new PriceLevelBFC().RetrieveByName(getValueFromCell(sheet.GetRow(row).GetCell(12))).ID;
        //                        soDetail.Price = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(14)).Replace("Rp","").Replace(".",""));
        //                        //soDetail.CreatedInvQuantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(9)));
        //                        SODItemNo++;
        //                        soDetails.Add(soDetail);

        //                        CreateSODetails(salesOrder.ID, soDetails);
        //                    }
        //                    //soDetail.Code = new CustomerBFC().GetCustomerCode();
        //                    //soDetail.Name = getValueFromCell(sheet.GetRow(row).GetCell(2));
        //                    //soDetail.Address = getValueFromCell(sheet.GetRow(row).GetCell(3));
        //                    //soDetail.IsActive = true;
                            
        //                }
        //            }
        //        }
                
        //    }
        //}

        //private void ImportBracelet()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Bracelet.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Bracelet.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "5";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(8)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(9));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(10));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(11)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 6; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportSettingBracelet()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/SettingBracelet.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/SettingBracelet.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "6";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(6)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(8));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(9));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(10)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 5; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportBrooch()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Brooch.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Brooch.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "11";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(5)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(6)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(7));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(8));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(9)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 4; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportSettingBrooch()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/SettingBrooch.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/SettingBrooch.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "12";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(5)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(6));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(7));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(8)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 3; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportEarring()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Earring.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Earring.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "9";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(5)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(6));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(7));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(8)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 3; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportSettingEarring()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/SettingEarring.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/SettingEarring.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "10";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(3)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(4)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(5));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(6));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(7)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 2; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportNecklace()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Necklace.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Necklace.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "3";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(8)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(9));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(10));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(11)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 6; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportSettingNecklace()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/SettingNecklace.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/SettingNecklace.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "4";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(6)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(8));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(9));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(10)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 5; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportSettingRing()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/SettingRing.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/SettingRing.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "8";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(7)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(8)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(9));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(10));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(11)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 6; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private void ImportRing()
        //{
        //    if (File.Exists(Server.MapPath("~/Import/Ring.xls")))
        //    {
        //        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/Import/Ring.xls")));
        //        ISheet sheet = workbook.GetSheetAt(0);

        //        //Create
        //        //for (int row = 5; row <= sheet.LastRowNum; row++)
        //        for (int row = 5; row <= sheet.LastRowNum; row++)
        //        {
        //            var product = new ProductModel();
        //            string typeID = "7";

        //            product.Code = new ProductBFC().GetProductCode(typeID);
        //            product.ProductName = getValueFromCell(sheet.GetRow(row).GetCell(0));
        //            product.AssetPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(8)));
        //            product.SellingPrice = Convert.ToDecimal(getValueFromCell(sheet.GetRow(row).GetCell(9)));
        //            product.Description = getValueFromCell(sheet.GetRow(row).GetCell(10));
        //            product.Type = typeID;
        //            product.IsActive = true;
        //            product.CreatedBy = product.ModifiedBy = "SYSTEM";
        //            product.CreatedDate = product.ModifiedDate = DateTime.Now;

        //            new ProductBFC().Create(product);

        //            var ProductLocationCount = new LYPOSDAC().RetrieveProductLocationCountByProductID(product.ID);

        //            var prodLocation = new ProductLocationModel();
        //            prodLocation.ProductID = product.ID;

        //            var locationCode = getValueFromCell(sheet.GetRow(row).GetCell(11));
        //            var location = new LocationBFC().RetrieveByCode(locationCode);

        //            prodLocation.ItemNo = ProductLocationCount + 1;
        //            prodLocation.LocationID = location.ID;
        //            prodLocation.Quantity = Convert.ToDouble(getValueFromCell(sheet.GetRow(row).GetCell(12)));

        //            new ProductLocationBFC().Create(prodLocation);

        //            var prodDetail = new ProductDetailModel();
        //            prodDetail.ProductID = product.ID;

        //            prodDetail.Price = product.SellingPrice;
        //            prodDetail.AssetPrice = product.AssetPrice;
        //            prodDetail.CreatedBy = prodDetail.ModifiedBy = product.CreatedBy;
        //            prodDetail.CreatedDate = prodDetail.ModifiedDate = DateTime.Now;

        //            new ProductDetailBFC().Create(prodDetail);

        //            for (int i = 1; i <= 7; i++)
        //            {
        //                var prodSpecification = new ProductSpecificationModel();
        //                prodSpecification.ProductID = product.ID;
        //                prodSpecification.TypeSpecificationID = i;
        //                prodSpecification.Value = getValueFromCell(sheet.GetRow(row).GetCell(i));
        //                new ProductSpecificationBFC().Create(prodSpecification);
        //            }

        //            var productDetail = new ProductBFC().RetrieveDetails(product.ID);

        //            product.StockQty = 0;

        //            foreach (var prodDet in productDetail)
        //            {
        //                product.StockQty += Convert.ToDouble(prodDet.Quantity);
        //            }
        //            new ProductBFC().Update(product);
        //        }
        //        //File.Delete(Server.MapPath("~/Import/Bracelet.xls"));
        //    }
        //}

        //private string getValueFromCell(NPOI.SS.UserModel.ICell Cell)
        //{
        //    string Value = "";
        //    try
        //    {
        //        if (Cell.CellType == CellType.STRING)
        //        {
        //            Value = Cell.StringCellValue;
        //        }
        //        else if (Cell.CellType == CellType.NUMERIC)
        //        {
        //            Value = Convert.ToString(Cell.NumericCellValue);
        //        }
        //        else
        //        {
        //            Value = Cell.StringCellValue;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //    return Value;
        //}

    }
}

