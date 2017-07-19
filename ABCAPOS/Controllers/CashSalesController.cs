﻿using System;
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
using System.Text;

namespace ABCAPOS.Controllers
{
    public class CashSalesController : MasterDetailController<CashSalesModel, CashSalesDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "CashSales";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetPreEditViewBag()
        {
            ViewBag.SalesmanList = new SalesmanBFC().Retrieve(true);

            //ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true).OrderBy(p => p.Name);

            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
        }

        public override MPL.Business.IMasterDetailBFC<CashSalesModel, CashSalesDetailModel> GetBFC()
        {
            return new CashSalesBFC();
        }

        private void SetPreCreateViewBag(CashSalesModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            //ViewBag.ExpedisiList = new ExpedisiBFC().RetrieveAll();
            //ViewBag.ConversionList = new List<UnitDetailModel>();
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
        }

        protected override void PreCreateDisplay(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);

            var cashSalesID = Request.QueryString["cashSalesID"];

            if (!string.IsNullOrEmpty(cashSalesID))
                new CashSalesBFC().CopyTransaction(header, Convert.ToInt64(cashSalesID));

            if (ViewBag.ErrorNotification != null)
            {
                new CashSalesBFC().ErrorTransaction(header, details);
                //ViewBag.ErrorNotification = null;
            }
            else
                header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());

            header.Code = SystemConstants.autoGenerated; //new SalesOrderBFC().GetSalesOrderCode();


            SetPreEditViewBag();

            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            foreach (var detail in details)
            {
                detail.HPP = detail.AssetPrice;
                double unitRate = 0;
                if (detail.ConversionID == 0)
                    unitRate = 1;
                else
                    unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                detail.StockQty = detail.StockQtyHidden / unitRate;
                detail.StockAvailable = detail.StockAvailableHidden / unitRate;
            }

            base.PreDetailDisplay(header, details);
        }

        protected void SetPreEditDetailViewBag(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.ConversionIDTemp = detail.ConversionID;
                detail.StockAvailable = detail.StockAvailableHidden;
                detail.StockQty = detail.StockQtyHidden;

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                var customer = new CustomerBFC().RetrieveByID(header.CustomerID);

                product = new SalesOrderBFC().GetSellingPrice(product, customer);
                detail.SaleUnitRateHidden = product.SaleUnitID;
                detail.PriceHidden = Convert.ToDouble(product.SellingPrice);
            }
        }

        protected override void PreUpdateDisplay(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            SetPreEditViewBag();
            SetPreEditDetailViewBag(header, details);

            base.PreUpdateDisplay(header, details);

            if (ViewBag.ErrorNotification != null)
            {
                RedirectToAction("Update", new { key = header.ID, ErrorMessage = ViewBag.ErrorNotification });
            }
        }

        public override void CreateData(CashSalesModel obj, List<CashSalesDetailModel> details)
        {
            try
            {
                new CashSalesBFC().Validate(obj, details);

                base.CreateData(obj, details);

                ApproveData(obj.ID.ToString());

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void ApproveData(string key)
        {
            base.ApproveData(key);
            new CashSalesBFC().UpdateInventory(key);
            new CashSalesBFC().UpdateStatus(Convert.ToInt64(key));

            //EmailPrintOut(Convert.ToInt64(key));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateCashSales(CashSalesModel obj, FormCollection col)
        {
            try
            {
                new CashSalesBFC().UpdateValidation(obj, obj.Details);

                base.Update(obj, col);

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Update;
                SetViewBagPermission();

                return RedirectToAction("Update", new { key = obj.ID, errorMessage = ex.Message });
            }
        }

        protected override List<Button> GetAdditionalButtons(CashSalesModel header, List<CashSalesDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            
            if (mode == UIMode.Detail && header.Status >= (int)MPL.DocumentStatus.Approved)
            {
                var invPrint = new Button();
                invPrint.Text = "Print Invoice";
                invPrint.CssClass = "button";
                invPrint.ID = "btnPrint";
                invPrint.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.Invoice, queryString = SystemConstants.str_CashSalesID + "=" + header.ID }));
                invPrint.Href = "#";
                list.Add(invPrint);
            }

            return list;
        }

        public override ActionResult Detail(string key, string errorMessage)
        {
            SetViewBagPermission();

            return base.Detail(key, errorMessage);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            var csCount = 0;
            var csList = new List<CashSalesModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            csCount = new CashSalesBFC().RetrieveCount(filter.GetSelectFilters());
            csList = new CashSalesBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = csCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(csList);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var cashSales = new CashSalesBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(cashSales);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(CashSalesModel cashSales, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new CashSalesBFC().Void(cashSales.ID, cashSales.VoidRemarks, MembershipHelper.GetUserName());
                new CashSalesBFC().UpdateStatus(Convert.ToInt64(cashSales.ID));
                //new EmailHelper().SendVoidSalesOrderEmail(salesOrder.ID, salesOrder.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                //if (voidFromIndex)
                return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = cashSales.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = cashSales.ID, errorMessage = ex.Message });
            }

        }
    }
}
