using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.ReportEDS;
using Microsoft.Reporting.WebForms;
using System.Data;
using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Util;
using ABCAPOS.Models;
using ABCAPOS.BF;

namespace ABCAPOS.Report.Accounting
{
    public partial class ProfitAndLoss : System.Web.UI.Page
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
            var startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
            var endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);

            var dt = new ABCAPOSReportDAC().RetrieveProfitAndLossReport(startDate, endDate);

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("CurrentDate", "Periode :" + startDate.ToString(SystemConstants.str_dateFormat) + " s/d " + endDate.ToString(SystemConstants.str_dateFormat)));
            ReportViewer1.LocalReport.SetParameters(param);

            var salesDT = dt.Where(p => p.RootParentCode == "4").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();
            var productionDT = dt.Where(p => p.RootParentCode == "5").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();
            var operationalDT = dt.Where(p => p.RootParentCode == "6").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();
            var incomeDT = dt.Where(p => p.RootParentCode == "7").CopyToDataTable<ABCAPOSReportEDSC.AccountBalanceDTRow>();

            var salesAmount = dt.Where(p => p.RootParentCode == "4").Sum(p => p.Balance);
            var productionAmount = dt.Where(p => p.RootParentCode == "5").Sum(p => p.Balance);
            var operationalAmount = dt.Where(p => p.RootParentCode == "6").Sum(p => p.Balance);
            var incomeAmount = dt.Where(p => p.RootParentCode == "7").Sum(p => p.Balance);

            var salesProfitDT = GetSalesProfit(salesAmount, productionAmount);
            var operationalProfitDT = GetOperationalProfit(salesAmount - productionAmount - operationalAmount);
            var profitDT = GetProfit(salesAmount - productionAmount - operationalAmount + incomeAmount);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesDT", salesDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ProductionDT", productionDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OperationalDT", operationalDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("IncomeDT", incomeDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesProfitDT", salesProfitDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OperationalProfitDT", operationalProfitDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ProfitDT", profitDT as DataTable));
            this.ReportViewer1.LocalReport.Refresh();
        }

        private void SetFilter()
        {
            string month = DateTime.Now.Month.ToString();
            DateTime startDate = DateTime.Now;
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString(SystemConstants.str_dateFormat);
            gffDate.TextBoxDateTo.Text = enddate.ToString(SystemConstants.str_dateFormat);
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

        private ABCAPOSReportEDSC.ParamDTDataTable GetSalesProfit(decimal salesAmount, decimal productionAmount)
        {
            var salesProfitDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            var grossProfit = salesAmount - productionAmount;

            var grossProfitDR = salesProfitDT.NewParamDTRow();
            grossProfitDR.Label = "Laba (Rugi) Kotor";
            grossProfitDR.Value = grossProfit.ToString("N2");
            salesProfitDT.LoadDataRow(grossProfitDR.ItemArray, false);

            var marginProfitDR = salesProfitDT.NewParamDTRow();
            marginProfitDR.Label = "Marjin Laba (Rugi) Kotor";

            if (salesAmount > 0)
                marginProfitDR.Value = (grossProfit * 100 / salesAmount).ToString("N0") + "%";
            else
                marginProfitDR.Value = "100%";

            salesProfitDT.LoadDataRow(marginProfitDR.ItemArray, false);

            return salesProfitDT;
        }

        private ABCAPOSReportEDSC.ParamDTDataTable GetOperationalProfit(decimal operationalProfitAmount)
        {
            var operationalProfitDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            var operationalProfitDR = operationalProfitDT.NewParamDTRow();
            operationalProfitDR.Label = "Laba (Rugi) Operasional Usaha";
            operationalProfitDR.Value = operationalProfitAmount.ToString("N2");
            operationalProfitDT.LoadDataRow(operationalProfitDR.ItemArray, false);

            return operationalProfitDT;
        }

        private ABCAPOSReportEDSC.ParamDTDataTable GetProfit(decimal profitAmount)
        {
            var profitDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            var profitDR = profitDT.NewParamDTRow();
            profitDR.Label = "Laba (Rugi) Bersih (Sebelum Pajak)";
            profitDR.Value = profitAmount.ToString("N2");
            profitDT.LoadDataRow(profitDR.ItemArray, false);

            return profitDT;
        }
    }
}