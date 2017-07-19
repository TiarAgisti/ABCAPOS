using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Transactions;
//using System.Transactions.Configuration;
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

namespace ABCAPOS
{
    public partial class PostingUlang : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                PostingUlangStock();
        }
        //private void Import()
        //{
           //ImportSalesOrderDetail();
            //importProduct();
            //importMarketing();
           //importVendor();
           //importStaff();
            //ImportDiamond("FancyColor", "13");
            //ImportWatch();
            //ImportBracelet();
            //ImportSettingBracelet();
            //ImportBrooch();
            //ImportSettingBrooch();
            //ImportEarring();
            //ImportSettingEarring();
            //ImportNecklace();
            //ImportSettingNecklace();
            //ImportRing();
            //ImportSettingRing();
            //ImportRapaport("Round");
            // ImportRapaport("Pear");
        //}

        private void PostingUlangStock()
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
                using (var ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(2, 0, 0)))
                {
                    //var details = new List<PostingUlangModel>();
                    var details = new ABCAPOSDAC().RetreiveDataPostingUlangStock();
                    foreach (var detail in details)
                    {
                        if (detail.Doctype == 2)
                        {
                            var receipt = new PurchaseDeliveryBFC().RetrieveByID(detail.ID);
                            var receiptDetails = new PurchaseDeliveryBFC().RetrieveDetails(detail.ID);
                            new PurchaseDeliveryBFC().CreateLog(receipt, receiptDetails);
                            new PurchaseDeliveryBFC().Update(receipt);
                        }

                        else if (detail.Doctype == 7)
                        {
                            var returnReceipt = new ReturnReceiptBFC().RetrieveByID(detail.ID);
                            var returnReceiptDetails = new ReturnReceiptBFC().RetrieveDetails(detail.ID);
                            new ReturnReceiptBFC().CreateLog(returnReceipt, returnReceiptDetails);
                            new ReturnReceiptBFC().Update(returnReceipt);
                        }

                        else if (detail.Doctype == 9)
                        {
                            var transferReceipt = new TransferReceiptBFC().RetrieveByID(detail.ID);
                            var transferReceiptDetails = new TransferReceiptBFC().RetrieveDetails(detail.ID);
                            new TransferReceiptBFC().CreateLog(transferReceipt, transferReceiptDetails);
                            new TransferReceiptBFC().Update(transferReceipt);
                        }

                        else if (detail.Doctype == 3)
                        {
                            var Build = new AssemblyBuildBFC().RetrieveByID(detail.ID);
                            var BuildDetails = new AssemblyBuildBFC().RetrieveDetails(detail.ID);
                            new AssemblyBuildBFC().IncreaseLog(Build, BuildDetails);
                            new AssemblyBuildBFC().Update(Build);
                        }

                        else if (detail.Doctype == 6)
                        {
                            var Inventory = new InventoryAdjustmentBFC().RetrieveByID(detail.ID);
                            var InventoryDetails = new InventoryAdjustmentBFC().RetrieveDetails(detail.ID);
                            new InventoryAdjustmentBFC().IncreaseLog(Inventory, InventoryDetails);
                            new InventoryAdjustmentBFC().Update(Inventory);
                        }

                        else if (detail.Doctype == 1)
                        {
                            var DeliveryOrder = new DeliveryOrderBFC().RetrieveByID(detail.ID);
                            //var DeliveryOrderDetails = new DeliveryOrderBFC().RetrieveDetails(detail.ID);
                            new DeliveryOrderBFC().CreateLog(DeliveryOrder);
                            //new DeliveryOrderBFC().Update(DeliveryOrder);
                        }

                        else if (detail.Doctype == 8)
                        {
                            var transferDelivery = new TransferDeliveryBFC().RetrieveByID(detail.ID);
                            new TransferDeliveryBFC().CreateLog(transferDelivery);
                            //new TransferDeliveryBFC().Update(transferDelivery);
                        }

                        else if (detail.Doctype == 10)
                        {
                            var vendorReturnDelivery = new VendorReturnDeliveryBFC().RetrieveByID(detail.ID);
                            new VendorReturnDeliveryBFC().CreateLog(vendorReturnDelivery);
                            //new VendorReturnDeliveryBFC().Update(vendorReturnDelivery);
                        }
                    }
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
                       
        }
      
        public void CreateSODetails(long soID, List<SalesOrderDetailModel> soDetails)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var soDetail in soDetails)
            {
                soDetail.SalesOrderID = soID;
                var itemNo = soDetail.ItemNo;
                soDetail.ItemNo = itemNo;

                SalesOrderDetail obj = new SalesOrderDetail();
                ObjectHelper.CopyProperties(soDetail, obj);
                ent.AddToSalesOrderDetail(obj);
            }

            ent.SaveChanges();
        }

       
        private string getValueFromCell(NPOI.SS.UserModel.ICell Cell)
        {
            string Value = "";
            try
            {
                if (Cell.CellType == CellType.STRING)
                {
                    Value = Cell.StringCellValue;
                }
                else if (Cell.CellType == CellType.NUMERIC)
                {
                    Value = Convert.ToString(Cell.NumericCellValue);
                }
                else
                {
                    Value = Cell.StringCellValue;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return Value;
        }

    }
}

