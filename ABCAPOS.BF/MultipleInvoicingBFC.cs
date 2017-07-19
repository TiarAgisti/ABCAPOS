using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using MPL;

namespace ABCAPOS.BF
{
    public class MultipleInvoicingBFC : MasterDetailBFC<MultipleInvoicing, v_MultipleInvoicing, MultipleInvoicingDetail, v_MultipleInvoicingDetail, MultipleInvoicingModel, MultipleInvoicingDetailModel>
    {
        private void NormalizeOldMultipleItemInvoice(long multipleInvID)
        {
            OnBeforeUpdated(multipleInvID);
            //new ABCAPOSDAC().DeleteMultipleInvoiceItem(multipleInvID);
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetMultipleInvoicingCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var multipleInvoicePrefix = "";

            if (prefixSetting != null)
                multipleInvoicePrefix = prefixSetting.MultipleInvoicingPrefix;

            var code = new ABCAPOSDAC().RetrieveMultipleInvoicingMaxCode(multipleInvoicePrefix, 7);

            return code;
        }

        protected override GenericDetailDAC<MultipleInvoicingDetail, MultipleInvoicingDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<MultipleInvoicingDetail, MultipleInvoicingDetailModel>("MultipleInvoicingID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_MultipleInvoicingDetail, MultipleInvoicingDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_MultipleInvoicingDetail, MultipleInvoicingDetailModel>("MultipleInvoicingID", "ItemNo", false);
        }

        protected override GenericDAC<MultipleInvoicing, MultipleInvoicingModel> GetMasterDAC()
        {
            return new GenericDAC<MultipleInvoicing, MultipleInvoicingModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_MultipleInvoicing, MultipleInvoicingModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_MultipleInvoicing, MultipleInvoicingModel>("ID", false, "Date DESC");
        }

        public void SetDateFromDateTo(MultipleInvoicingModel header)
        {
            string month = DateTime.Now.Month.ToString();
            DateTime startDate = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);
            //DateTime enddate = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            DateTime date = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            header.DateFrom = startDate;
            header.DateTo = enddate;
            header.Date = date;
        }

        public void Validate(MultipleInvoicingModel obj, List<MultipleInvoicingDetailModel> details)
        {
            obj.Amount = 0;
            obj.TaxAmount = 0;

            foreach (var detail in details)
            {
                var invoice = new InvoiceBFC().RetrieveByID(detail.InvoiceID);
                obj.Amount += invoice.Amount;
                obj.TaxAmount += invoice.TaxAmount;
            }

            if (details.Count == 0)
                throw new Exception("Pilih minimal 1 invoice");
        }

        public void CreateByCustomerID(MultipleInvoicingModel mInv, long customerID, string dateFrom, string dateTo, string date)
        {
            var customer = new CustomerBFC().RetrieveByID(customerID);
            
            if (string.IsNullOrEmpty(dateFrom))
            {
                SetDateFromDateTo(mInv);
            }
            else
            {
                DateTime startDate = Convert.ToDateTime(dateFrom);
                DateTime enddate = Convert.ToDateTime(dateTo);
                DateTime dateASA = Convert.ToDateTime(date);
                mInv.DateFrom = startDate;
                mInv.DateTo = enddate;
                mInv.Date = dateASA;
                
            }

            if (customer != null)
            {

                mInv.CustomerID = customerID;
                mInv.CustomerName = customer.Name;
                mInv.BillingAddress1 = customer.BillingAddress1;
                var details = new List<MultipleInvoicingDetailModel>();
                var invoices = new InvoiceBFC().RetrieveByCustomerIDStartEndDate(customerID, mInv.DateFrom, mInv.DateTo);

                foreach (var invoice in invoices)
                {
                    //if (invoice.HasMultiple == false || new InvoiceBFC().RetrieveDetails(invoice.ID).Where(p => p.HasInvoiceItem == false).Count() > 0)
                    if (invoice.HasMultiple == false)
                    {
                        var detail = new MultipleInvoicingDetailModel();

                        ObjectHelper.CopyProperties(invoice, detail);
                        detail.InvoiceCode = invoice.Code;
                        detail.InvoiceID = invoice.ID;
                        details.Add(detail);

                        mInv.SubTotal += detail.Amount;
                        mInv.TaxValue += detail.TaxAmount;
                        mInv.GrandTotal += detail.GrandTotal;
                    }
                }

                mInv.Details = details;
            }
        }

        public void UpdateByCustomerID(MultipleInvoicingModel mInv, List<MultipleInvoicingDetailModel> details, long customerID, string dateFrom, string dateTo)
        {
            var customer = new CustomerBFC().RetrieveByID(customerID);

            if (string.IsNullOrEmpty(dateFrom))
            {
                SetDateFromDateTo(mInv);
            }
            else
            {
                
                DateTime startDate = Convert.ToDateTime(dateFrom);
                DateTime enddate = Convert.ToDateTime(dateTo);
                mInv.DateFrom = startDate;
                mInv.DateTo = enddate;
            }

            if (customer != null)
            {
                //mInv.Code = SystemConstants.autoGenerated;//GetMultipleInvoicingCode();

                mInv.CustomerID = customerID;
                mInv.CustomerName = customer.Name;
                mInv.BillingAddress1 = customer.BillingAddress1;
                //var details = new List<MultipleInvoicingDetailModel>();
                var invoices = new InvoiceBFC().RetrieveByCustomerIDStartEndDate(customerID, mInv.DateFrom, mInv.DateTo);

                foreach (var invoice in invoices)
                {
                    //if (invoice.HasMultiple == false || new InvoiceBFC().RetrieveDetails(invoice.ID).Where(p => p.HasInvoiceItem == false).Count() > 0)
                    if (invoice.HasMultiple == false)
                    {
                        var detail = new MultipleInvoicingDetailModel();

                        ObjectHelper.CopyProperties(invoice, detail);
                        detail.InvoiceCode = invoice.Code;
                        detail.InvoiceID = invoice.ID;
                        details.Add(detail);
                    }
                }

                mInv.Details = details;
            }
        }

        public void OnCreated(long multiInvID)
        {
            var multiInvDetails = RetrieveDetails(multiInvID);
            //var multiInvItemDetails = Retrieve
            //var multiInv = RetrieveByID(multiInvID);

            if (multiInvDetails != null)
            {
                foreach (var multiInvDetail in multiInvDetails)
                {
                    var invoice = new InvoiceBFC().RetrieveByID(multiInvDetail.InvoiceID);
                    invoice.HasMultiple = true;

                    new InvoiceBFC().Update(invoice);
                }
            }

            var multiInvItems = RetrieveItemDetails(multiInvID);

            foreach (var multiInvItem in multiInvItems)
            {
                var invoiceDetails = new InvoiceBFC().RetrieveDetails(multiInvItem.InvoiceID);
                new InvoiceBFC().UpdateInvoiceDetailInvoiceItem(multiInvItem, invoiceDetails, true);
            }
        }

        public void OnVoid(long multiInvID)
        {
            var multiInvDetails = RetrieveDetails(multiInvID);
            if (multiInvDetails != null)
            {
                foreach (var multiInvDetail in multiInvDetails)
                {
                    var invoice = new InvoiceBFC().RetrieveByID(multiInvDetail.InvoiceID);
                    invoice.HasMultiple = false;

                    new InvoiceBFC().Update(invoice);
                }
            }
        }

        public void OnBeforeUpdated(long multiInvID)
        {
            var multiInvDetails = RetrieveDetails(multiInvID);
            //var multiInvItemDetails = Retrieve
            //var multiInv = RetrieveByID(multiInvID);

            if (multiInvDetails != null)
            {
                foreach (var multiInvDetail in multiInvDetails)
                {
                    var invoice = new InvoiceBFC().RetrieveByID(multiInvDetail.InvoiceID);
                    invoice.HasMultiple = true;

                    new InvoiceBFC().Update(invoice);
                }
            }

            var multiInvItems = RetrieveItemDetails(multiInvID);

            foreach (var multiInvItem in multiInvItems)
            {
                var invoiceDetails = new InvoiceBFC().RetrieveDetails(multiInvItem.InvoiceID);
                new InvoiceBFC().UpdateInvoiceDetailInvoiceItem(multiInvItem, invoiceDetails, false);
            }
        }

        public void CreateMultipleInvItem(long multipleInvID, List<MultipleInvoiceItemModel> itemDetails)
        {
            var dac = new ABCAPOSDAC();
            var itemNo = 1;
            new ABCAPOSDAC().DeleteMultipleInvoiceItem(multipleInvID);

            foreach (var itemDet in itemDetails)
            {
                itemDet.MultipleInvoicingID = multipleInvID;
                itemDet.ItemNo = itemNo++;

                dac.CreateMultipleInvoiceItem(itemDet);
            }
        }

        public override void Create(MultipleInvoicingModel header, List<MultipleInvoicingDetailModel> details)
        {
            header.Code = GetMultipleInvoicingCode();
            
            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);
                if (header.ItemDetails.Count > 0)
                    CreateMultipleInvItem(header.ID, header.ItemDetails);
                else
                {
                    //CreateNullMultipleInvItem(header, details);
                    CreateMultipleInvItem(header.ID, header.ItemDetails);
                }

                //OnCreated(header.ID);

                trans.Complete();
            }
            OnCreated(header.ID);
            //new SalesOrderBFC().UpdateStatus(header.SalesOrderID, header.CreatedBy);

        }

        public void Update(MultipleInvoicingModel header, List<MultipleInvoicingDetailModel> details, string userName)
        {
            var itemDetails = header.ItemDetails;
            header.Status = (int)MPL.DocumentStatus.New;
            header.DueDate = header.Date;
            header.ModifiedBy = header.ApprovedBy = userName;
            header.ModifiedDate = header.ApprovedDate = DateTime.Now;
            NormalizeOldMultipleItemInvoice(header.ID);
            using (TransactionScope trans = new TransactionScope())
            {
                //NormalizeOldMultipleItemInvoice(header.ID);
                
                base.Update(header, details);
                CreateMultipleInvItem(header.ID, itemDetails);
                //OnCreated(header.ID);

                trans.Complete();
            }
            OnCreated(header.ID);
        }

        public void Void(MultipleInvoicingModel header, string userName)
        {
            var obj = RetrieveByID(header.ID);
            obj.VoidRemarks = header.VoidRemarks;
            obj.Status = (int)MPL.DocumentStatus.Void;
            base.Update(obj);

            new ABCAPOSDAC().DeleteMultipleInvoiceItem(header.ID);

            this.OnVoid(header.ID);
        }

        public List<MultipleInvoiceItemModel> RetrieveAllMultipleInvoiceItem(long multiInvoiceID, long invoiceID)
        {
            var invoiceDetails = new InvoiceBFC().RetrieveDetails(invoiceID);

            //int itemNo = 1;
            List<MultipleInvoiceItemModel> listItem = new List<MultipleInvoiceItemModel>();
            foreach (var invDet in invoiceDetails)
            {
                if (invDet.HasInvoiceItem == true)
                {
                    MultipleInvoiceItemModel itemDetail = new MultipleInvoiceItemModel();
                    itemDetail.MultipleInvoicingID = multiInvoiceID;
                    //itemDetail.ItemNo = itemNo;
                    itemDetail.InvoiceID = invoiceID;
                    itemDetail.InvoiceDetailItemNo = invDet.ItemNo;
                    itemDetail.TaxType = invDet.TaxType;
                    itemDetail.ProductID = invDet.ProductID;
                    itemDetail.ProductName = invDet.ProductName;
                    itemDetail.Quantity = invDet.Quantity;
                    itemDetail.ConversionName = invDet.ConversionName;
                    itemDetail.Price = invDet.Price;

                    var grossAmount = invDet.GrossAmount;
                    if (grossAmount == 0)
                    {
                        itemDetail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(itemDetail.Price) * Convert.ToDecimal(itemDetail.Quantity)).ToString("N2"));
                        if (itemDetail.TaxType == 2)
                            itemDetail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(itemDetail.TotalAmount) * 0.1).ToString("N2"));
                        else
                            itemDetail.TotalPPN = 0;
                        itemDetail.GrossAmount = itemDetail.TotalAmount + itemDetail.TotalPPN;
                    }
                    else
                    {
                        itemDetail.GrossAmount = grossAmount;
                        if (itemDetail.TaxType == 2)
                        {
                            itemDetail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(itemDetail.GrossAmount) / 1.1).ToString("N2"));
                            itemDetail.TotalPPN = itemDetail.GrossAmount - itemDetail.TotalAmount;
                        }
                        else
                        {
                            itemDetail.TotalAmount = itemDetail.GrossAmount;
                            itemDetail.TotalPPN = 0;
                        }
                    }
                    listItem.Add(itemDetail);
                    //itemNo++;
                }
            }
            return listItem;
        }

        public List<MultipleInvoiceItemModel> RetrieveMultipleInvoiceItem(long multiInvoiceID, long invoiceID)
        {
            var invoiceDetails = new InvoiceBFC().RetrieveDetails(invoiceID);

            //int itemNo = 1;
            List<MultipleInvoiceItemModel> listItem = new List<MultipleInvoiceItemModel>();
            foreach (var invDet in invoiceDetails)
            {
                if (invDet.HasInvoiceItem == false)
                {
                    MultipleInvoiceItemModel itemDetail = new MultipleInvoiceItemModel();
                    itemDetail.MultipleInvoicingID = multiInvoiceID;
                    //itemDetail.ItemNo = itemNo;
                    itemDetail.InvoiceID = invoiceID;
                    itemDetail.InvoiceDetailItemNo = invDet.ItemNo;
                    itemDetail.TaxType = invDet.TaxType;
                    itemDetail.ProductID = invDet.ProductID;
                    itemDetail.ProductName = invDet.ProductName;
                    itemDetail.Quantity = invDet.Quantity;
                    itemDetail.ConversionName = invDet.ConversionName;
                    itemDetail.Price = invDet.Price;

                    var grossAmount = invDet.GrossAmount;
                    if (grossAmount == 0)
                    {
                        itemDetail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(itemDetail.Price) * Convert.ToDecimal(itemDetail.Quantity)).ToString("N2"));
                        if (itemDetail.TaxType == 2)
                            itemDetail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(itemDetail.TotalAmount) * 0.1).ToString("N2"));
                        else
                            itemDetail.TotalPPN = 0;
                        itemDetail.GrossAmount = itemDetail.TotalAmount + itemDetail.TotalPPN;
                    }
                    else
                    {
                        itemDetail.GrossAmount = grossAmount;
                        if (itemDetail.TaxType == 2)
                        {
                            itemDetail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(itemDetail.GrossAmount) / 1.1).ToString("N2"));
                            itemDetail.TotalPPN = itemDetail.GrossAmount - itemDetail.TotalAmount;
                        }
                        else
                        {
                            itemDetail.TotalAmount = itemDetail.GrossAmount;
                            itemDetail.TotalPPN = 0;
                        }
                    }
                    listItem.Add(itemDetail);
                    //itemNo++;
                }
            }
            return listItem;
        }

        public List<MultipleInvoiceItemModel> RetrieveItemDetails(long multiInvID)
        {
            return new ABCAPOSDAC().RetrieveMultipleInvoiceItems(multiInvID);
        }

        public List<MultipleInvoiceItemModel> RetrieveItemDetailsGroup(long multiInvID)
        {
            //multipleInvoiceItem.GroupBy(c => c.ProductID).Select(grp => grp.Key);
            return new ABCAPOSDAC().RetrieveMultipleInvoiceItemsGroup(multiInvID);
        }

        public ABCAPOSReportEDSC.InvoiceDTRow RetrieveMultipleInvoicePrintOut(long multipleInvoiceID)
        {
            return new ABCAPOSReportDAC().RetrieveMultipleInvoicePrintOut(multipleInvoiceID);
        }

        public ABCAPOSReportEDSC.TaxInvoiceDTRow RetrieveMultipleInvoiceTaxInvoicePrintOut(long multipleInvoicingID)
        {
            return new ABCAPOSReportDAC().RetrieveMultipleInvoicingPrintOut(multipleInvoicingID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long multipleInvoicingID)
        {
            return new ABCAPOSReportDAC().RetrieveMultipleInvoicingDetailPrintOut(multipleInvoicingID);
        }

    }
}
