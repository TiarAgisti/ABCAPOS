using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.DA;
using MPL;
using Microsoft.Reporting.WebForms;
using MPL.Reporting;
using ABCAPOS.EDM;
using ABCAPOS.Util;
using ABCAPOS.Models;
using System.Data;
using ABCAPOS.BF;

namespace ABCAPOS.Report.Accounting
{
    public partial class AccountTransaction : System.Web.UI.Page
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
            long accountID = 0;
            var account = new AccountModel();

            if (gffAccount.Selected)
            {
                accountID = Convert.ToInt64(gffAccount.DropDownList.SelectedValue);

                account = new AccountBFC().RetrieveByID(accountID);
            }

            var dt = new ABCAPOSReportDAC().RetrieveAccountTransactionReport(startDate, endDate, accountID);

            //foreach (var dr in dt)
            //{
            //    if (dr.Balance >= 0)
            //    {
            //        dr.DebitBalance = dr.Balance;
            //        dr.CreditBalance = 0;
            //    }
            //    else
            //    {
            //        dr.CreditBalance = dr.Balance;
            //        dr.DebitBalance = 0;
            //    }
            //}

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("Period", startDate.ToString(SystemConstants.str_dateFormat) + " s/d " + endDate.ToString(SystemConstants.str_dateFormat)));

            if (gffAccount.Selected)
                param.Add(new ReportParameter("Account", account.Name));

            ReportViewer1.LocalReport.SetParameters(param);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AccountTransactionDT", dt as DataTable));
            this.ReportViewer1.LocalReport.Refresh();
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

        private void SetFilter()
        {
            string month = DateTime.Now.Month.ToString();
            DateTime startDate = Convert.ToDateTime("1/" + month + "/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString(SystemConstants.str_dateFormat);
            gffDate.TextBoxDateTo.Text = enddate.ToString(SystemConstants.str_dateFormat);

            DropDownList ddlAccount = gffAccount.DropDownList as DropDownList;
            ddlAccount.Items.Clear();

            var accountCodeList = new AccountBFC().RetrieveAll().OrderBy(p=>p.Name);
            foreach (var accountCode in accountCodeList)
            {
                ListItem t = new ListItem();
                t = new ListItem(accountCode.Name, Convert.ToString(accountCode.ID));
                ddlAccount.Items.Add(t);
            }
        }

        private string GenerateFromXML()
        {
            var rpt = new TableReport();
            rpt.DataSourceItemType = typeof(AccountTransactionModel);
            ReportHelper.ReadFromXML(rpt, SystemConstants.ReportXMLFolder + "Accounting/ReportXML/AccountTransaction.xml");

            rpt.Filters = ReportHelper.GetReportFilters(GenericFilter1);
            return ReportHelper.GenerateReport(rpt);
        }
    }
}