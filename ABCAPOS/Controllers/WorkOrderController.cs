using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.Helpers;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class WorkOrderController : MasterDetailController<WorkOrderModel, WorkOrderDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "WorkOrder";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);
            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
            ViewBag.AllowViewFG = roleDetails.Contains("ViewFinishGood");
            ViewBag.AllowViewALL = roleDetails.Contains("ViewALL");
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
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();
        }

        private void SetPreCreateViewBag(WorkOrderModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            //ViewBag.ConversionList = new List<UnitDetailModel>();
        }

        private void SetViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.ConversionList = new List<UnitDetailModel>();
        }

        private void SetViewBagDetail(WorkOrderModel header, List<WorkOrderDetailModel> details)
        {
            var productID = Request.QueryString["ProductID"];
            var Qty = Request.QueryString["Qty"];
            var salesorderID = Request.QueryString["SalesOrderID"];
            if (!string.IsNullOrEmpty(productID))
                new WorkOrderBFC().CreatedByFormulasi(header, Convert.ToInt64(productID), Convert.ToDecimal(Qty), Convert.ToInt64(salesorderID));
        }

        public override MPL.Business.IMasterDetailBFC<WorkOrderModel, WorkOrderDetailModel> GetBFC()
        {
            return new WorkOrderBFC();
        }

        protected override List<Button> GetAdditionalButtons(WorkOrderModel header, List<WorkOrderDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            var print = new Button();
            print.Text = "Print Bill Of Material";
            print.CssClass = "button";
            print.ID = "btnPrint";
            print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                new { type = ReportViewerController.PrintOutType.WorkOrder, queryString = SystemConstants.str_WorkOrderID + "=" + header.ID }));
            print.Href = "#";

            var printLetter = new Button();
            printLetter.Text = "Print WO ½ Letter";
            printLetter.CssClass = "button";
            printLetter.ID = "btnPrintLetter";
            printLetter.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                new { type = ReportViewerController.PrintOutType.WorkOrderLetter, queryString = SystemConstants.str_WorkOrderID + "=" + header.ID }));
            printLetter.Href = "#";

            var printStruk = new Button();
            printStruk.Text = "Print Struk";
            printStruk.CssClass = "button";
            printStruk.ID = "btnPrintLetter";
            printStruk.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                new { type = ReportViewerController.PrintOutType.WorkOrderStruk, queryString = SystemConstants.str_WorkOrderID + "=" + header.ID }));
            printStruk.Href = "#";

            var Build = new Button();
            Build.Text = "Build";
            Build.CssClass = "button";
            Build.ID = "btnBuild";
            Build.Href = Url.Content("~/AssemblyBuild/Create?workOrderID="+header.ID);
           
            if (mode == UIMode.Detail && header.Status == (int)MPL.DocumentStatus.Approved)
            {
                list.Add(print);
                list.Add(printLetter);
                list.Add(printStruk);
                list.Add(Build);
            }
            else if (mode == UIMode.Detail && header.Status == (int)WorkOrderStatus.New || header.Status == (int)WorkOrderStatus.FullyBuild || header.Status == (int)WorkOrderStatus.PartialyBuild)
            {
                list.Add(print);
                list.Add(printLetter);
                list.Add(printStruk);
            }

            return list;
        }

        protected override void PreCreateDisplay(WorkOrderModel header, List<WorkOrderDetailModel> details)
        {
            this.SetViewBagPermission();
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            SetViewBagDetail(header, details);

            //header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());
            //ViewBag.Conversion = new FormulasiBFC().RetreiveByProductID(header.ProductID);
            header.Code = SystemConstants.autoGenerated;
            //header.Details = details.OrderBy(p => p.ItemNo);
            base.PreCreateDisplay(header, details);

        }

        protected override void PreDetailDisplay(WorkOrderModel header, List<WorkOrderDetailModel> details)
        {
            SetViewBag();

            this.SetViewBagPermission();

            SetViewBagNotification();

            SetViewBagDetail(header, details);

            ViewBag.BuildList = new AssemblyBuildBFC().RetreiveBuildByWOID(header.ID);

            header.Details = new WorkOrderBFC().RetrieveDetails(header.ID).OrderBy(e=>e.ItemNo).ToList();

            base.PreDetailDisplay(header, header.Details);
        }

        protected override void PreUpdateDisplay(WorkOrderModel header, List<WorkOrderDetailModel> details)
        {
            SetViewBag();

            this.SetViewBagPermission();

            SetViewBagNotification(); 
  
            //header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());
            //ViewBag.Conversion = new UnitBFC().Retrieve(true);
            var woDetails = new FormulasiBFC().RetreiveByProductID(header.ProductID);
            header.Details = new WorkOrderBFC().RetrieveDetails(header.ID).OrderBy(e => e.ItemNo).ToList();
            foreach (var detail in header.Details)
            {
                try
                {
                    detail.StockQtyHidden = woDetails.Where(p => p.ProductDetailID == detail.ProductDetailID).FirstOrDefault().Qty;
                }
                catch (Exception ex)
                {
                    detail.StockQtyHidden = 1;
                }
              
            }
           
            base.PreUpdateDisplay(header, header.Details);
        }

        protected void SetPreEditDetailViewBag(WorkOrderModel header, List<WorkOrderDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.ConversionIDTemp = detail.ConversionID;
            }
        }

        public override void CreateData(WorkOrderModel obj, List<WorkOrderDetailModel> details)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    base.CreateData(obj, details);

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
               
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        public override void UpdateData(WorkOrderModel obj, List<WorkOrderDetailModel> details, FormCollection formCollection)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    base.UpdateData(obj, details, formCollection);

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Dokumen berhasil diupdate";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                //RedirectToAction("Update", new { key = obj.ID, ErrorMessage = ex.Message});
                throw;

            }
        }

        public override void ApproveData(string key)
        {

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new WorkOrderBFC().Approve(Convert.ToInt32(key), MembershipHelper.GetUserName());

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been approved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
            //base.ApproveData(key);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            this.SetViewBagPermission();
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();

            var woCount = 0;
            var woList = new List<WorkOrderModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            woCount = new WorkOrderBFC().RetreiveCount(filter.GetSelectFilters(), true,ViewBag);
            woList = new WorkOrderBFC().Retreive((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), true,ViewBag);

            ViewBag.DataCount = woCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            //ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(woList);
          
            //return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult ApproveFromIndex(string key)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    ApproveData(key);
                    //UIMode mode;
                    trans.Complete();

                    return RedirectToAction("Index");
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var workOrder = new WorkOrderBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(workOrder);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(WorkOrderModel workOrder, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new WorkOrderBFC().Void(workOrder, MembershipHelper.GetUserName());

                    trans.Complete();
                }
                
                //TempData["SuccessNotification"] = "Document has been canceled";

                return RedirectToAction("Index");

                //if (voidFromIndex)
                //    return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = workOrder.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = workOrder.ID, errorMessage = ex.Message });
            }
        }

    }
}
