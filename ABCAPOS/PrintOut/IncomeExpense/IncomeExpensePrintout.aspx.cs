using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using Microsoft.Reporting.WebForms;
using ABCAPOS.DA;
using ABCAPOS.BF;
using System.Data;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.PrintOut.IncomeExpense
{
    public partial class IncomeExpensePrintout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_IncomeExpenseID] != null)
                {
                    long incomeExpenseID = Convert.ToInt64(Request.QueryString[SystemConstants.str_IncomeExpenseID]);

                    var incomeExpenseDT = new ABCAPOSReportEDSC.IncomeExpenseDTDataTable();
                    var incomeExpenseDR = new IncomeExpenseBFC().RetrievePrintOut(incomeExpenseID);

                    var incomeExpenseDetailDT = new IncomeExpenseBFC().RetrieveDetailPrintOut(incomeExpenseID);
                    //incomeExpenseDR.ItemAmount = incomeExpenseDetailDT.Sum(p => p.Quantity);

                    incomeExpenseDT.LoadDataRow(incomeExpenseDR.ItemArray, true);

                    //var companySetting = new CompanySettingBFC().Retrieve();
                    
                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("IncomeExpenseDT", incomeExpenseDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = incomeExpenseDR.Code;
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (Request.QueryString[SystemConstants.str_IncomeExpenseID] != null)
            {
                long incomeExpenseID = Convert.ToInt64(Request.QueryString[SystemConstants.str_IncomeExpenseID]);

                var incomeExpense = new IncomeExpenseBFC().RetrieveByID(incomeExpenseID);
                var incomeExpenseDetailDT = new IncomeExpenseBFC().RetrieveDetailPrintOut(incomeExpenseID);

                e.DataSources.Add(new ReportDataSource("IncomeExpenseDetailDT", incomeExpenseDetailDT as DataTable));
            }
        }
    }
}