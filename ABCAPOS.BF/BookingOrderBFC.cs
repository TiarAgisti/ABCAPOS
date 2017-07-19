using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using MPL;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class BookingOrderBFC : MasterDetailBFC<BookingOrder, v_BookingOrder, BookingOrderDetail, v_BookingOrderDetail, BookingOrderModel, BookingOrderDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        #region Retrieve 
        
        public string RetrieveBookingOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BookingOrder
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

        public string GetBookingOrderCode(BookingOrderModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var bookingPrefix = "";

            if (prefixSetting != null)
                bookingPrefix = prefixSetting.BookingPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = bookingPrefix + year + "-" + warehouse.Code + "-";
            var code = RetrieveBookingOrderMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<BookingOrderDetail, BookingOrderDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<BookingOrderDetail, BookingOrderDetailModel>("BookingOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_BookingOrderDetail, BookingOrderDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_BookingOrderDetail, BookingOrderDetailModel>("BookingOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<BookingOrder, BookingOrderModel> GetMasterDAC()
        {
            return new GenericDAC<BookingOrder, BookingOrderModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_BookingOrder, BookingOrderModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_BookingOrder, BookingOrderModel>("ID", false, "Date DESC");
        }

        #endregion

        #region Transaction 

        public void CopyTransaction(BookingOrderModel header, long bookingOrderID)
        {
            var bookingOrder = RetrieveByID(bookingOrderID);
            var bookingOrderDetails = RetrieveDetails(bookingOrderID);

            ObjectHelper.CopyProperties(bookingOrder, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<BookingOrderDetailModel>();

            foreach (var bookingOrderDetail in bookingOrderDetails)
            {
                var detail = new BookingOrderDetailModel();

                ObjectHelper.CopyProperties(bookingOrderDetail, detail);

                details.Add(detail);
            }

            header.Details = details;
        }

        public void ErrorTransaction(BookingOrderModel header, List<BookingOrderDetailModel> details)
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

        #region Create Booking

        public void Validate(BookingOrderModel obj, List<BookingOrderDetailModel> details)
        {
            decimal POTotal = 0;

            foreach (var detail in details)
            {
                detail.Discount = 0;
                detail.Remarks = "";
                detail.Price = 0;

                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Product cannot be zero");

                var total = Convert.ToDecimal(detail.Quantity) * detail.AssetPrice;
                POTotal = Convert.ToDecimal(total);

                // Bug Fix: allow free items
                //if (POTotal == 0)
                //    throw new Exception("Total must be higher than zero");
            }
        }

        public override void Create(BookingOrderModel header, List<BookingOrderDetailModel> details)
        {
            header.Code = GetBookingOrderCode(header);
            header.IsPurchaseable = true;
            header.StatusDescription = "New";

            base.Create(header, details);
        }
        #endregion

        #region Update Booking

        public void UpdateValidation(BookingOrderModel obj, List<BookingOrderDetailModel> details)
        {
        }

        public void UpdateStatus(long bookingOrderID)
        {
            var booking = RetrieveByID(bookingOrderID);
            //var bookingList = new BookingOrderBFC().RetrieveDetails(bookingOrderID);
            //var qtyBooking = bookingList.Sum(p => p.Quantity);
            var poList = new PurchaseOrderBFC().RetrieveByBOID(bookingOrderID);
            var qtyPO = poList.Sum(p => p.Quantity);

            if (qtyPO == 0)
            {
                booking.IsPurchaseable = true;
                booking.StatusDescription = "New";
            }
            else if (qtyPO == booking.Quantity)
            {
                booking.IsPurchaseable = false;
                booking.StatusDescription = "Fully Booked";
            }
            else
            {
                booking.IsPurchaseable = true;
                booking.StatusDescription = "Partially Booked";
            }
            Update(booking);
        }

        #endregion 

        #region Void Booking

        public void Void(long bookingOrderID, string voidRemarks, string userName)
        {
            var bookingOrder = RetrieveByID(bookingOrderID);
            var oldStatus = bookingOrder.Status;
            bookingOrder.Status = (int)MPL.DocumentStatus.Void;
            bookingOrder.VoidRemarks = voidRemarks;
            bookingOrder.ApprovedDate = DateTime.Now;
            bookingOrder.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(bookingOrder);
                trans.Complete();
            }
        }

        #endregion 

        public List<BookingOrderModel> Retrieve(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveBookingOrder(startIndex, amount, sortParameter, selectFilters, showVoidDocuments);
        }

        public int RetrieveCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveBookingOrderCount(selectFilters, showVoidDocuments);
        }
    }
}
