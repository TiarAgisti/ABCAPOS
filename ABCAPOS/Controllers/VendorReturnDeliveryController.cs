using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class VendorReturnDeliveryController : MasterDetailController<VendorReturnDeliveryModel, VendorReturnDeliveryDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "VendorReturnDelivery";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetPreCreateViewBag(VendorReturnDeliveryModel header)
        {
            ViewBag.DriverList = new StaffBFC().Retrieve("Distribution", true);

            var SJReturnCheckboxList = "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='" + header.SJReturn + "' ></td></tr>";
            ViewBag.SJReturnCheckboxList = SJReturnCheckboxList;

        }

        private void SetViewBagDetail(VendorReturnDeliveryModel header)
        {
            var SJReturnCheckboxList = "";
            if (header.SJReturn == 0)
            {
                SJReturnCheckboxList += "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='0'  disabled ></td></tr>";
            }
            else
            {
                SJReturnCheckboxList += "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='1' checked disabled ></td></tr>";
            }
            ViewBag.SJReturnCheckboxList = SJReturnCheckboxList;

        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        private void SetDetail(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                if (unitRate == 0)
                {
                    unitRate = 1;
                }
                detail.StockQty = detail.StockQty / unitRate;
                detail.StockAvailable = detail.StockAvailable / unitRate;
            }
        }

        private void ReadData(VendorReturnDeliveryModel oj, List<VendorReturnDeliveryDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (!string.IsNullOrEmpty(detail.StrQuantity))
                    detail.Quantity = Convert.ToDouble(detail.StrQuantity);
                else
                    detail.Quantity = 0;
            }
        }

        public override MPL.Business.IMasterDetailBFC<VendorReturnDeliveryModel, VendorReturnDeliveryDetailModel> GetBFC()
        {
            return new VendorReturnDeliveryBFC();
        }

        protected void SetPreEditDetailViewBag(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.Quantity == 0)
                    detail.StrQuantity = "";
                else
                    detail.StrQuantity = detail.Quantity.ToString("N0");

                detail.QtyDO = detail.QtyDO - detail.Quantity;
                detail.QtyHidden = detail.QtySO - detail.QtyDO;

                detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLoc != null)
                    detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                else
                    detail.StockQty = 0;

                if (itemLoc != null)
                    detail.StockAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                else
                    detail.StockAvailable = 0;
            }
        }

        protected override void PreCreateDisplay(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            SetPreCreateViewBag(header);
            ViewBag.BinList = new BinBFC().RetrieveAll();
            var vendorReturnID = Request.QueryString["vendorReturnID"];

            if (!string.IsNullOrEmpty(vendorReturnID))
                new VendorReturnDeliveryBFC().CreateByVendorReturn(header, Convert.ToInt64(vendorReturnID));

            header.Code = SystemConstants.autoGenerated;//new VendorReturnDeliveryBFC().GetVendorReturnDeliveryCode();

            base.PreCreateDisplay(header, header.Details);
        }
        
        protected override void PreDetailDisplay(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            SetViewBagDetail(header);

            this.SetDetail(header, details);

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            SetPreCreateViewBag(header);

            SetPreEditDetailViewBag(header, details);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(VendorReturnDeliveryModel obj, List<VendorReturnDeliveryDetailModel> details)
        {
            //DeliveryOrderStatus statusOnCreate = (DeliveryOrderStatus)obj.Status;
            // Assume StatusOnCreate = Shipped all the time
            DeliveryOrderStatus statusOnCreate = (DeliveryOrderStatus)obj.Status;
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new VendorReturnDeliveryBFC().Validate(obj, details);

                    if (Request.Form["SJReturn"] != null)
                        obj.SJReturn = 1;

                    base.CreateData(obj, details);

                    //obj.Status = (int)statusOnCreate;

                    new VendorReturnDeliveryBFC().ImplementStatus(obj.ID, MembershipHelper.GetUserName(), DeliveryOrderStatus.Void, statusOnCreate);

                    trans.Complete();

                    TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(VendorReturnDeliveryModel obj, List<VendorReturnDeliveryDetailModel> details, FormCollection formCollection)
        {
            try
            {
                using(TransactionScope trans = new TransactionScope())
                {
                    //new VendorReturnDeliveryBFC().Validate(obj, details);
                    ReadData(obj, obj.Details);
                    var oldData = new VendorReturnDeliveryBFC().RetrieveByID(obj.ID);
                    var bfc = new VendorReturnDeliveryBFC();

                    bfc.ImplementStatus(obj.ID, MembershipHelper.GetUserName(), (DeliveryOrderStatus)oldData.Status, DeliveryOrderStatus.Void);

                    if (Request.Form["SJReturn"] != null)
                        obj.SJReturn = 1;
                    else
                        obj.SJReturn = 0;

                    base.UpdateData(obj, details, formCollection);

                    bfc.ImplementStatus(obj.ID, MembershipHelper.GetUserName(), DeliveryOrderStatus.Void, (DeliveryOrderStatus)obj.Status);

                    trans.Complete();

                    TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void ApproveData(string key)
        {
            new VendorReturnDeliveryBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
        }

        public override void VoidData(string key)
        {
            new VendorReturnDeliveryBFC().Void(Convert.ToInt64(key),"", MembershipHelper.GetUserName());
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        //public ActionResult Pack(string key)
        //{
        //    var bfc = new VendorReturnDeliveryBFC();
        //    var vendorReturnDelivery = bfc.RetrieveByID(key);
        //    bfc.ImplementStatus(Convert.ToInt64(key)
        //        , MembershipHelper.GetUserName()
        //        , (DeliveryOrderStatus)vendorReturnDelivery.Status
        //        , DeliveryOrderStatus.Packed);

        //    return RedirectToAction("Detail", new { key = vendorReturnDelivery.ID });
        //}

        public ActionResult Ship(string key)
        {
            var bfc = new VendorReturnDeliveryBFC();
            var vendorReturnDelivery = bfc.RetrieveByID(key);
            bfc.ImplementStatus(Convert.ToInt64(key)
                , MembershipHelper.GetUserName()
                , (DeliveryOrderStatus)vendorReturnDelivery.Status
                , DeliveryOrderStatus.Shipped);

            return RedirectToAction("Detail", new { key = vendorReturnDelivery.ID });
        }

        public ActionResult Pick(string key)
        {
            var bfc = new VendorReturnDeliveryBFC();
            var vendorReturnDelivery = bfc.RetrieveByID(key);
            bfc.ImplementStatus(Convert.ToInt64(key)
                , MembershipHelper.GetUserName()
                , (DeliveryOrderStatus)vendorReturnDelivery.Status
                , DeliveryOrderStatus.New);

            return RedirectToAction("Detail", new { key = vendorReturnDelivery.ID });
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var vendorReturnDelivery = new VendorReturnDeliveryBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(vendorReturnDelivery);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(VendorReturnDeliveryModel vendorReturnDelivery, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {

                var header = new VendorReturnDeliveryBFC().RetrieveByID(vendorReturnDelivery.ID);
                DeliveryOrderStatus OldStatus = (DeliveryOrderStatus)header.Status;
                DeliveryOrderStatus NewStatus = (int)DeliveryOrderStatus.Void;

                new VendorReturnDeliveryBFC().ImplementStatus(header.ID, MembershipHelper.GetUserName(), OldStatus, NewStatus);
                new VendorReturnDeliveryBFC().Void(vendorReturnDelivery.ID, vendorReturnDelivery.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidDOEmail(vendorReturnDelivery.ID, vendorReturnDelivery.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = vendorReturnDelivery.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = vendorReturnDelivery.ID, errorMessage = ex.Message });
            }

        }
    }
}
