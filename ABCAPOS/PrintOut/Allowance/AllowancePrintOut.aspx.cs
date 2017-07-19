using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using ABCAPOS.DA;
using ABCAPOS.BF;
using Microsoft.Reporting.WebForms;
using System.Data;

namespace ABCAPOS.PrintOut.Allowance
{
    public partial class AllowancePrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_AllowanceID] != null)
                {
                    long allowanceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_AllowanceID]);

                    var allowance = new AllowanceBFC().RetrieveByID(allowanceID);
                    var allowanceDetailDT = new ABCAPOSReportDAC().RetrieveAllowanceDetailPrintOut(allowanceID);

                    foreach (var allowanceDetailDR in allowanceDetailDT)
                    {
                        allowanceDetailDR.IncomeAmount = allowanceDetailDR.BasicSalary + allowanceDetailDR.TransportAllowance + allowanceDetailDR.ActiveBonus + allowanceDetailDR.PositionAllowance + allowanceDetailDR.MealAllowance + allowanceDetailDR.Bonus + allowanceDetailDR.THR;
                        allowanceDetailDR.DeductionAmount = allowanceDetailDR.MealAllowanceExpense + allowanceDetailDR.PaidLoanAmount;
                    }

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Letterhead/Allowance.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("CurrentMonth", allowance.Date.ToString("MMMM yyyy")));
                    param.Add(new ReportParameter("ImagePath", path));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AllowanceDetailDT", allowanceDetailDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = "Slip Gaji " + allowance.Date.ToString("MMMM yyyy");
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}