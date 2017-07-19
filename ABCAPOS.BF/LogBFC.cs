using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;
using System.Transactions;
using MPL;

namespace ABCAPOS.BF
{
    public class LogBFC:MasterDetailBFC<Log,v_Log,LogDetail,v_LogDetail,LogModel,LogDetailModel>
    {
        protected override GenericDetailDAC<LogDetail, LogDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<LogDetail, LogDetailModel>("LogID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_LogDetail, LogDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_LogDetail, LogDetailModel>("LogID", "ItemNo", false);
        }

        protected override GenericDAC<Log, LogModel> GetMasterDAC()
        {
            return new GenericDAC<Log, LogModel>("ID", false, "DATE");
        }

        protected override GenericDAC<v_Log, LogModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_Log, LogModel>("ID", false, "DATE");
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public override void Create(LogModel header, List<LogDetailModel> details)
        {
            base.Create(header, details);
        }

        public override void Update(LogModel header, List<LogDetailModel> details)
        {
            base.Update(header, details);
        }

        public LogDetailModel RetreiveByLogIDContainerIDProductIDWarehouseID(long logID,long containerID, long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetreiveLogByLogIDContainerIDProductIDWarehouseID(logID, containerID, productID, warehouseID);
        }

        public List<LogDetailModel> RetreiveLogByPurchaseOrderID(long purchaseorderID)
        {
            return new ABCAPOSDAC().RetreiveLogByPurchaseOrderID(purchaseorderID);
        }

        public LogDetailModel RetreiveLogByLogIDProductID(long logID, long productID)
        {
            return new ABCAPOSDAC().RetreiveLogByLogIDProductID(logID, productID);
        }

        public LogDetailModel RetreivePriceByLogIDProductIDWarehouseID(long logID, long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetreivePriceByLogIDProductIDWarehouseID(logID, productID, warehouseID);
        }

        public List<LogDetailModel> RetreiveProductQtyOnLogDetail(long productID, long warehouseID, DateTime date)
        {
            return new ABCAPOSDAC().RetreiveProductQtyOnLogDetail(productID, warehouseID, date);
        }

        public List<StockMovementModel> RetrieveStockMovement(long productID, int transactionType)
        {
            return new ABCAPOSDAC().RetrieveStockMovement(productID, transactionType);
        }

        public List<StockMovementModel> RetrieveStockMovement(long productID, int transactionType,DateTime startDate,DateTime endDate, int startIndex, int amount)
        {
            return new ABCAPOSDAC().RetrieveStockMovement(productID, transactionType,startDate,endDate, startIndex, amount);
        }

        public int RetrieveStockMovementCount(long productID, int transactionType, DateTime startDate, DateTime endDate)
        {
            return new ABCAPOSDAC().RetrieveStockMovementCount(productID, transactionType, startDate, endDate);
        }

        //Report Goods Sold By Product
        public List<LogDetailModel> RetrieveProductSold(DateTime startDate, DateTime endDate, string productCode, string productName)
        {
            return new ABCAPOSReportDAC().RetrieveProductSold(startDate, endDate, productCode, productName);
        }

        //Report Goods Sold By Customer
        public List<LogDetailModel> RetrieveProductSoldByCustomer(DateTime startDate, DateTime endDate, string productCode, string productName, string customerName, string salesReference)
        {
            return new ABCAPOSReportDAC().RetrieveProductSoldByCustomer(startDate, endDate, productCode, productName, customerName, salesReference);
        }

        //Report Goods Returned By Customer
        public List<LogDetailModel> RetrieveProductReturnedByCustomer(DateTime startDate, DateTime endDate, string productCode, string productName, string customerName)
        {
            return new ABCAPOSReportDAC().RetrieveProductReturnedByCustomer(startDate, endDate, productCode, productName, customerName);
        }
    }
}
