using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class DeliveryOrderController : MasterDetailController<DeliveryOrderModel, DeliveryOrderDetailModel>
    {
       
        private string ModuleID
        {
            get
            {
                return "DeliveryOrder";
            }
        }

        private void ReadData(DeliveryOrderModel oj, List<DeliveryOrderDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (!string.IsNullOrEmpty(detail.StrQuantity))
                    detail.Quantity = Convert.ToDouble(detail.StrQuantity);
                else
                    detail.Quantity = 0;
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetPreCreateViewBag(DeliveryOrderModel header)
        {
            ViewBag.DriverList = new StaffBFC().Retrieve("Distribution", true);
            ViewBag.ExpedisiList = new ExpeditionBFC().RetrieveActive();

            var SJReturnCheckboxList = "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='" + header.SJReturn + "' ></td></tr>";
            ViewBag.SJReturnCheckboxList = SJReturnCheckboxList;
           

        }

        private void SetPreUpdateViewBag(DeliveryOrderModel header)
        {
            ViewBag.DriverList = new StaffBFC().Retrieve("Distribution", true);
            var SJReturnCheckboxList = "";
            if (header.SJReturn == 0)
            {
                SJReturnCheckboxList += "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' ></td></tr>";
            }
            else
            {
                SJReturnCheckboxList += "<tr><td>SJ Kembali</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' checked ></td></tr>";
            }
            ViewBag.SJReturnCheckboxList = SJReturnCheckboxList;
            ViewBag.ExpedisiList = new ExpeditionBFC().RetrieveActive();
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
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }

        private void SetViewBagDetail(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            var SJReturnCheckboxList = "";
            if (header.SJReturn == 0)
            {
                SJReturnCheckboxList += "<tr><td>Invoice Payment Account</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='0'  disabled ></td></tr>";
            }
            else
            {
                SJReturnCheckboxList += "<tr><td>Invoice Payment Account</td><td>:</td><td colspan='5'><input type='checkbox' name='SJReturn' value='1' checked disabled ></td></tr>";
            }
            ViewBag.SJReturnCheckboxList = SJReturnCheckboxList;

            var removeZeroDetails = new List<DeliveryOrderDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Quantity > 0)
                    removeZeroDetails.Add(detail);

                detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);

                if (itemLoc != null)
                    detail.StockAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                else
                    detail.StockAvailable = 0;

                if (itemLoc != null)
                    detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                else
                    detail.StockQty = 0;
            }
            details = new List<DeliveryOrderDetailModel>();
            header.Details = removeZeroDetails;
        }

        protected void SetPreEditDetailViewBag(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.Quantity > 0)
                    detail.isFulFill = true;

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

        protected override List<Button> GetAdditionalButtons(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail && header.Status != (int)MPL.DocumentStatus.Void)
            {
                var print = new Button();
                print.Text = "Print Surat Jalan";
                print.CssClass = "button";
                print.ID = "btnPrint";
                print.OnClick = "if (confirm('Are you sure you want to print this document?')) { " + String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.DeliveryOrder, queryString = SystemConstants.str_DeliveryOrderID + "=" + header.ID })) + " } ";
                print.Href = "#";
                list.Add(print); //TODO: reimplement printing

            }

            return list;
        }

        protected override void PreCreateDisplay(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            ViewBag.BinList = new BinBFC().RetrieveAll();
            var salesOrderID = Request.QueryString["salesOrderID"];

            if (!string.IsNullOrEmpty(salesOrderID))
                new DeliveryOrderBFC().CreateBySalesOrder(header, Convert.ToInt64(salesOrderID));

            header.Code = SystemConstants.autoGenerated;//new DeliveryOrderBFC().GetDeliveryOrderCode();

            base.PreCreateDisplay(header, header.Details);
        }

        protected override void PreUpdateDisplay(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetPreUpdateViewBag(header);
            SetPreEditDetailViewBag(header, details);

            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            SetViewBagDetail(header, details);

            base.PreDetailDisplay(header, details);
        }

        public override MPL.Business.IMasterDetailBFC<DeliveryOrderModel, DeliveryOrderDetailModel> GetBFC()
        {
            return new DeliveryOrderBFC();
        }

        public override void UpdateData(DeliveryOrderModel obj, List<DeliveryOrderDetailModel> details, FormCollection formCollection)
        {
            try
            {
                ReadData(obj, obj.Details);
                var oldData = new DeliveryOrderBFC().RetrieveByID(obj.ID);
                var bfc = new DeliveryOrderBFC();
                // cancel old data from inventory
                bfc.ImplementStatus(obj.ID, MembershipHelper.GetUserName(), (DeliveryOrderStatus)oldData.Status, DeliveryOrderStatus.Void);

                // disabling validations for now
                //new DeliveryOrderBFC().Validate(obj, details);
                if (Request.Form["SJReturn"] != null)
                    obj.SJReturn = 1;
                else
                    obj.SJReturn = 0;

                base.UpdateData(obj, details, formCollection);
                // apply new data to inventory
                bfc.ImplementStatus(obj.ID, MembershipHelper.GetUserName(), DeliveryOrderStatus.Void, (DeliveryOrderStatus)obj.Status);

                TempData["SuccessNotification"] = "Document successfully saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void ApproveData(string key)
        {
            new DeliveryOrderBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
        }

        public override void VoidData(string key)
        {
            new DeliveryOrderBFC().Void(Convert.ToInt64(key), "", MembershipHelper.GetUserName());
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        //public ActionResult Pack(string key)
        //{
        //    var bfc = new DeliveryOrderBFC();
        //    var deliveryOrder = bfc.RetrieveByID(key);
        //    bfc.ImplementStatus(Convert.ToInt64(key)
        //        , MembershipHelper.GetUserName()
        //        , (DeliveryOrderStatus)deliveryOrder.Status
        //        , DeliveryOrderStatus.Packed);

        //    return RedirectToAction("Detail", new { key = deliveryOrder.ID });
        //}

        public ActionResult Ship(string key)
        {
            var bfc = new DeliveryOrderBFC();
            var deliveryOrder = bfc.RetrieveByID(key);
            try
            {
                new DeliveryOrderBFC().ValidateShip(Convert.ToInt64(key));
                bfc.ImplementStatus(Convert.ToInt64(key)
                    , MembershipHelper.GetUserName()
                    , (DeliveryOrderStatus)deliveryOrder.Status
                    , DeliveryOrderStatus.Shipped);

                deliveryOrder.Status = (int)DeliveryOrderStatus.Shipped;
                bfc.Update(deliveryOrder);

                return RedirectToAction("Detail", new { key = deliveryOrder.ID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Detail", new { key = deliveryOrder.ID, errorMessage = ex.Message });
            }

        }

        public ActionResult Pick(string key)
        {
            var bfc = new DeliveryOrderBFC();
            var deliveryOrder = bfc.RetrieveByID(key);
            bfc.ImplementStatus(Convert.ToInt64(key)
                , MembershipHelper.GetUserName()
                , (DeliveryOrderStatus)deliveryOrder.Status
                , DeliveryOrderStatus.New);
            deliveryOrder.Status = (int)DeliveryOrderStatus.New;
            bfc.Update(deliveryOrder);

            return RedirectToAction("Detail", new { key = deliveryOrder.ID });
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var deliveryOrder = new DeliveryOrderBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(deliveryOrder);
            }

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateDeliveryOrder(DeliveryOrderModel obj, List<DeliveryOrderDetailModel> details)
        {
            DeliveryOrderStatus statusOnCreate = (DeliveryOrderStatus)obj.Status;
            obj.Details = details;
            ReadData(obj, obj.Details);

            try
            {
                new DeliveryOrderBFC().Validate(obj, obj.Details);

                if (Request.Form["SJReturn"] != null)
                    obj.SJReturn = 1;
                base.Create(obj);
                obj.Status = (int)statusOnCreate;

                new DeliveryOrderBFC().ImplementStatus(obj.ID, MembershipHelper.GetUserName(), DeliveryOrderStatus.Void, statusOnCreate);
                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { salesOrderID = obj.SalesOrderID, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult VoidRemarks(DeliveryOrderModel deliveryOrder, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {

                var header = new DeliveryOrderBFC().RetrieveByID(deliveryOrder.ID);
                DeliveryOrderStatus OldStatus = (DeliveryOrderStatus)header.Status;
                DeliveryOrderStatus NewStatus = (int)DeliveryOrderStatus.Void;

                new DeliveryOrderBFC().ImplementStatus(header.ID, MembershipHelper.GetUserName(), OldStatus,NewStatus);

                new DeliveryOrderBFC().Void(header.ID,deliveryOrder.VoidRemarks, MembershipHelper.GetUserName());

                return RedirectToAction("Index");

                //new EmailHelper().SendVoidDOEmail(deliveryOrder.ID, deliveryOrder.VoidRemarks, MembershipHelper.GetUserName());

                //TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                //if (voidFromIndex)
                //    return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = deliveryOrder.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = deliveryOrder.ID, errorMessage = ex.Message });
            }

        }

    }
}
