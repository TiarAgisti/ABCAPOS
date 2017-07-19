using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.ReportEDS;
using MPL;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class BookingSalesBFC : MasterDetailBFC<BookingSales, v_BookingSales, BookingSalesDetail, v_BookingSalesDetail, BookingSalesModel, BookingSalesDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        #region Retrieve

        public string RetrieveBookingSalesMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BookingSales
                        where i.Code.StartsWith(prefix) 
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public string GetBookingSalesCode(BookingSalesModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var bookingSalesPrefix = "";

            if (prefixSetting != null)
                bookingSalesPrefix = prefixSetting.BookingSalesPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = bookingSalesPrefix + year + "-" + warehouse.Code + "-";
            var code = RetrieveBookingSalesMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<BookingSalesDetail, BookingSalesDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<BookingSalesDetail, BookingSalesDetailModel>("BookingSalesID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_BookingSalesDetail, BookingSalesDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_BookingSalesDetail, BookingSalesDetailModel>("BookingSalesID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<BookingSales, BookingSalesModel> GetMasterDAC()
        {
            return new GenericDAC<BookingSales, BookingSalesModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_BookingSales, BookingSalesModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_BookingSales, BookingSalesModel>("ID", false, "Date DESC");
        }

        #endregion

        #region Transaction

        public void CopyTransaction(BookingSalesModel header, long bookingSalesID)
        {
            var bookingSales = RetrieveByID(bookingSalesID);
            var bookingSalesDetails = RetrieveDetails(bookingSalesID);

            ObjectHelper.CopyProperties(bookingSales, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<BookingSalesDetailModel>();

            foreach (var bookingSalesDetail in bookingSalesDetails)
            {
                var detail = new BookingSalesDetailModel();

                ObjectHelper.CopyProperties(bookingSalesDetail, detail);

                details.Add(detail);
            }

            header.Details = details;
        }

        public void ErrorTransaction(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {

            foreach (var detail in details)
            {
                var product = new ProductBFC().RetrieveByID(detail.ProductID);

                if (product != null)
                    detail.ProductName = product.ProductName;
            }

            header.Details = details;
        }

        #endregion

        #region Create Booking Sales

        public void Validate(BookingSalesModel obj, List<BookingSalesDetailModel> details)
        {
            
            foreach (var detail in details)
            {
                detail.Discount = 0;
                detail.Remarks = "";
                detail.AssetPrice = 0;

                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Product cannot be zero");

                var total = Convert.ToDecimal(detail.Quantity) * detail.Price;
               
            }
        }

        public override void Create(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            header.Code = GetBookingSalesCode(header);
            header.IsSaleable = true;
            header.StatusDescription = "New";

            base.Create(header, details);
        }
        #endregion

        #region Update Booking

        public void UpdateValidation(BookingSalesModel obj, List<BookingSalesDetailModel> details)
        {
        }

        public void UpdateStatus(long bookingSalesID)
        {
            var booking = RetrieveByID(bookingSalesID);
            //var bookingList = new BookingSalesBFC().RetrieveDetails(bookingSalesID);
            //var qtyBooking = bookingList.Sum(p => p.Quantity);
            var soList = new SalesOrderBFC().RetrieveByBSID(bookingSalesID);
            var qtySO = soList.Sum(p => p.Quantity);

            if (qtySO == 0)
            {
                booking.IsSaleable = true;
                booking.StatusDescription = "New";
            }
            else if (qtySO == booking.Quantity)
            {
                booking.IsSaleable = false;
                booking.StatusDescription = "Fully Booked";
            }
            else
            {
                booking.IsSaleable = true;
                booking.StatusDescription = "Partially Booked";
            }
            Update(booking);
        }

        #endregion

        #region Void Booking

        public void Void(long bookingSalesID, string voidRemarks, string userName)
        {
            var bookingSales = RetrieveByID(bookingSalesID);
            var oldStatus = bookingSales.Status;
            bookingSales.Status = (int)MPL.DocumentStatus.Void;
            bookingSales.StatusDescription = "Void";
            bookingSales.VoidRemarks = voidRemarks;
            bookingSales.ApprovedDate = DateTime.Now;
            bookingSales.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(bookingSales);
                trans.Complete();
            }
        }

        #endregion 

        #region Print Booking

        public ABCAPOSReportEDSC.SalesOrderDTRow RetrievePrintOut(long bookingSalesID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingSales
                        where i.ID == bookingSalesID
                        select i;

            ABCAPOSReportEDSC.SalesOrderDTRow dr = new ABCAPOSReportEDSC.SalesOrderDTDataTable().NewSalesOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.SalesOrderDetailDTDataTable RetrieveDetailPrintOut(long bookingSalesID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingSalesDetail
                        where i.BookingSalesID == bookingSalesID
                        orderby i.LineSequenceNumber
                        select i;

            ABCAPOSReportEDSC.SalesOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.SalesOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        #endregion 

        public List<BookingSalesModel> Retrieve(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveBookingSales(startIndex, amount, sortParameter, selectFilters, showVoidDocuments);
        }

        public int RetrieveCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveBookingSalesCount(selectFilters, showVoidDocuments);
        }
    }
}
