using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ABCAPOS.Util;
using System.Xml.Linq;
using MPL.Reporting;
using System.Globalization;
using MPL.MVC;

namespace ABCAPOS.Report
{
    public class ReportHelper
    {
        private static void ClearReports()
        {
            foreach (string fileName in Directory.GetFiles(SystemConstants.TempReportFolder))
            {
                try
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string timeStamp = fileName.Substring(fileName.LastIndexOf("\\") + 1, 10);

                    DateTime date = DateTime.ParseExact(timeStamp, "yyyyMMddHH", provider);

                    if (date < DateTime.Now.AddHours(-1))
                        File.Delete(fileName);
                }
                catch
                {
                    File.Delete(fileName);
                }
            }
        }

        public static string GenerateReport(TableReport report)
        {
            ClearReports();
            string timeStamp = DateTime.Now.ToString("yyyyMMddHH");
            string fileName = SystemConstants.TempReportFolder + timeStamp + Guid.NewGuid() + ".rdlc";

            var content = report.Render();

            File.WriteAllText(fileName, report.Render());

            return fileName;
        }

        public static void ReadFromXML(TableReport report, string xmlFileName)
        {
            var usCulture = new CultureInfo("en-US");
            XDocument doc = XDocument.Load(xmlFileName);

            if (doc.Root.Attribute("Title") != null)
                report.Title = doc.Root.Attribute("Title").Value;

            var chart = doc.Descendants("Chart").FirstOrDefault();
            if (chart != null)
            {
                var reportChart = new ReportChart();

                if (chart.Attribute("XAxisField") != null)
                    reportChart.XAxisFieldName = chart.Attribute("XAxisField").Value;

                if (chart.Attribute("XAxisLabelField") != null)
                    reportChart.XAxisLabelFieldName = chart.Attribute("XAxisLabelField").Value;

                if (chart.Attribute("LegendLabelFieldName") != null)
                    reportChart.LegendLabelFieldName = chart.Attribute("LegendLabelFieldName").Value;

                if (chart.Attribute("YAxisField") != null)
                    reportChart.YAxisFieldName = chart.Attribute("YAxisField").Value;

                if (chart.Attribute("Width") != null)
                    reportChart.Height = float.Parse(chart.Attribute("Width").Value, usCulture);

                if (chart.Attribute("Height") != null)
                    reportChart.Height = float.Parse(chart.Attribute("Height").Value, usCulture);

                if (chart.Attribute("Title") != null)
                    reportChart.Title = chart.Attribute("Title").Value;

                if (chart.Attribute("Type") != null)
                    reportChart.ChartType = chart.Attribute("Type").Value;

                if (chart.Attribute("ShowLegend") != null)
                    reportChart.ShowLegend = bool.Parse(chart.Attribute("ShowLegend").Value);

                report.Chart = reportChart;
            }

            var columns = doc.Descendants("Column");
            foreach (var column in columns)
            {
                var tableColumn = new TableDataColumn();

                if (column.Attribute("Field") != null)
                    tableColumn.FieldName = column.Attribute("Field").Value;

                if (column.Attribute("HeaderText") != null)
                    tableColumn.HeaderText = column.Attribute("HeaderText").Value;

                if (column.Attribute("Width") != null)
                    tableColumn.Width = float.Parse(column.Attribute("Width").Value, usCulture);

                if (column.Attribute("TextAlign") != null)
                    tableColumn.TextAlign = column.Attribute("TextAlign").Value;

                if (column.Attribute("Format") != null)
                    tableColumn.Format = column.Attribute("Format").Value;

                report.Columns.Add(tableColumn);
            }

            var footers = doc.Descendants("Footer");
            foreach (var footer in footers)
            {
                var tableFooter = new TableDataFooterRow();
                var cell1 = new TableDataFooterCell();
                cell1.TextAlign = "Right";
                cell1.ColSpan = columns.Count() - 1;

                if (footer.Attribute("LabelSpan") != null)
                {
                    int span = Convert.ToInt32(footer.Attribute("LabelSpan").Value);
                    cell1.ColSpan = span;
                }

                if (footer.Attribute("Label") != null)
                    cell1.Value = footer.Attribute("Label").Value;

                tableFooter.FooterCells.Add(cell1);

                for (int x = 0; x < 20; x++)
                {
                    var cell2 = new TableDataFooterCell();
                    cell2.TextAlign = "Right";
                    cell2.Format = "N2";

                    string indexStr = "";

                    if (x > 0)
                        indexStr = Convert.ToString(x + 1);

                    if (footer.Attribute("Format" + indexStr) != null)
                        cell2.Format = footer.Attribute("Format" + indexStr).Value;

                    if (footer.Attribute("Content" + indexStr) != null)
                    {
                        cell2.Value = footer.Attribute("Content" + indexStr).Value;
                        tableFooter.FooterCells.Add(cell2);
                    }
                    else
                        break;
                }
                report.FooterRows.Add(tableFooter);
            }

            var group1 = doc.Descendants("Group1").FirstOrDefault();
            if (group1 != null)
            {
                if (group1.Attribute("Field") != null)
                    report.GroupByField1 = group1.Attribute("Field").Value;

                if (group1.Attribute("Header") != null)
                    report.GroupByRowContent1 = group1.Attribute("Header").Value;

                if (group1.Attribute("SortBy") != null)
                    report.GroupBy1SortField = group1.Attribute("SortBy").Value;

                if (group1.Attribute("SortDescending") != null)
                    report.GroupBy1SortDescending = Convert.ToBoolean(group1.Attribute("SortDescending").Value);

                if (group1.Attribute("FooterLabel") != null && group1.Attribute("FooterContent") != null)
                {
                    TableDataFooterRow groupFooterRow = new TableDataFooterRow();

                    TableDataFooterCell cell1 = new TableDataFooterCell();
                    cell1.ColSpan = columns.Count() - 1;

                    if (group1.Attribute("FooterLabelSpan") != null)
                    {
                        int span = Convert.ToInt32(group1.Attribute("FooterLabelSpan").Value);
                        cell1.ColSpan = span;
                    }

                    cell1.Value = group1.Attribute("FooterLabel").Value;
                    groupFooterRow.FooterCells.Add(cell1);

                    for (int x = 0; x < 20; x++)
                    {
                        string indexStr = "";

                        if (x > 0)
                            indexStr = Convert.ToString(x + 1);

                        if (group1.Attribute("FooterContent" + indexStr) != null)
                        {
                            TableDataFooterCell cell2 = new TableDataFooterCell();
                            cell2.Value = group1.Attribute("FooterContent" + indexStr).Value;
                            cell2.Format = "N2";

                            if (group1.Attribute("Format" + indexStr) != null)
                                cell2.Format = group1.Attribute("Format" + indexStr).Value;

                            groupFooterRow.FooterCells.Add(cell2);
                        }
                        else
                            break;
                    }

                    report.GroupFooterRows1.Add(groupFooterRow);
                }
            }

            var group2 = doc.Descendants("Group2").FirstOrDefault();
            if (group2 != null)
            {
                if (group2.Attribute("Field") != null)
                    report.GroupByField2 = group2.Attribute("Field").Value;

                if (group2.Attribute("Header") != null)
                    report.GroupByRowContent2 = group2.Attribute("Header").Value;

                if (group2.Attribute("SortBy") != null)
                    report.GroupBy2SortField = group2.Attribute("SortBy").Value;

                if (group2.Attribute("SortDescending") != null)
                    report.GroupBy2SortDescending = Convert.ToBoolean(group2.Attribute("SortDescending").Value);

                if (group2.Attribute("FooterLabel") != null && group2.Attribute("FooterContent") != null)
                {
                    TableDataFooterRow groupFooterRow = new TableDataFooterRow();

                    TableDataFooterCell cell1 = new TableDataFooterCell();
                    cell1.ColSpan = columns.Count() - 1;

                    if (group2.Attribute("FooterLabelSpan") != null)
                    {
                        int span = Convert.ToInt32(group2.Attribute("FooterLabelSpan").Value);
                        cell1.ColSpan = span;
                    }

                    cell1.Value = group2.Attribute("FooterLabel").Value;
                    groupFooterRow.FooterCells.Add(cell1);

                    for (int x = 0; x < 20; x++)
                    {
                        string indexStr = "";

                        if (x > 0)
                            indexStr = Convert.ToString(x + 1);

                        if (group2.Attribute("FooterContent" + indexStr) != null)
                        {
                            TableDataFooterCell cell2 = new TableDataFooterCell();
                            cell2.Value = group2.Attribute("FooterContent" + indexStr).Value;
                            cell2.Format = "N2";

                            if (group2.Attribute("Format" + indexStr) != null)
                                cell2.Format = group2.Attribute("Format" + indexStr).Value;

                            groupFooterRow.FooterCells.Add(cell2);
                        }
                        else
                            break;
                    }


                    report.GroupFooterRows2.Add(groupFooterRow);
                }
            }

        }

        public static List<ReportFilter> GetReportFilters(GenericFilter genericFilter)
        {
            List<ReportFilter> filters = new List<ReportFilter>();
            foreach (GenericFilterField gff in genericFilter.FilterFields.Controls)
            {
                if (gff.Selected)
                {
                    var filter = new ReportFilter();
                    filter.Label = gff.FieldText;

                    if (gff.Type == FieldType.List)
                    {
                        if (gff.DropDownList.SelectedItem != null)
                            filter.Value = gff.DropDownList.SelectedItem.Text;
                    }
                    else if (gff.Type == FieldType.DateRange)
                        filter.Value = gff.TextBoxDateFrom.Text + " s/d " + gff.TextBoxDateTo.Text;
                    else if (gff.Type == FieldType.Date)
                        filter.Value = gff.TextBoxDate.Text;
                    else
                        filter.Value = gff.TextBoxString.Text;

                    filters.Add(filter);
                }
            }

            return filters;
        }
    }
}