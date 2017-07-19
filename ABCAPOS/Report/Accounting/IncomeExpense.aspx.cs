using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.ReportEDS;
using ABCAPOS.Models;
using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Util;
using System.Data;
using Microsoft.Reporting.WebForms;
using ABCAPOS.BF;

namespace ABCAPOS.Report.Accounting
{
    public partial class IncomeExpense : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetFilter();
                SetDataSource();
            }
        }

        private void SetDataSource()
        {
            DateTime endDate = Convert.ToDateTime(gffDate.TextBoxDate.Text);
            DateTime startDate = Convert.ToDateTime("1/1/" + endDate.Year.ToString());

            var dt = new ABCAPOSReportDAC().RetrieveIncomeExpenseReport(startDate, endDate);

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("CurrentDate", "Tanggal :" + endDate.ToString(SystemConstants.str_dateFormat)));
            ReportViewer1.LocalReport.SetParameters(param);

            var wealthDT = dt.Where(p => p.RootParentCode == "1").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();
            var assetDT = dt.Where(p => p.RootParentCode != "1").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("WealthDT", wealthDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AssetDT", assetDT as DataTable));
            this.ReportViewer1.LocalReport.Refresh();
        }

        private void SetFilter()
        {
            //string month = DateTime.Now.Month.ToString();
            //DateTime startDate = Convert.ToDateTime("1/1/" + DateTime.Now.Year);
            //DateTime nowDate = Convert.ToDateTime("1/" + month + "/" + DateTime.Now.Year);
            //DateTime enddate = (nowDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDate.Text = DateTime.Now.ToString(SystemConstants.str_dateFormat);
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }
    }
}