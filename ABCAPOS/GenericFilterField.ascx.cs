using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using MPL.Business;

namespace ABCAPOS
{
    public partial class GenericFilterField : System.Web.UI.UserControl
    {
        public bool Selected
        {
            get
            {
                return chkSelected.Checked;
            }
            set
            {
                chkSelected.Checked = value;
            }
        }

        public string FieldText
        {
            get
            {
                return lblFieldText.Text;
            }
            set
            {
                lblFieldText.Text = value;
            }
        }

        public string FieldName
        {
            get
            {
                return hdnField.Value;
            }
            set
            {
                hdnField.Value = value;
            }
        }

        public DropDownList DropDownList
        {
            get
            {
                return ddlList;
            }
        }

        public TextBox TextBoxDate
        {
            get
            {
                return txtDate;
            }
        }

        public TextBox TextBoxString
        {
            get
            {
                return txtText;
            }
        }

        public TextBox TextBoxDateFrom
        {
            get
            {
                return txtDateFrom;
            }
        }

        public TextBox TextBoxDateTo
        {
            get
            {
                return txtDateTo;
            }
        }

        public TextBox TextBoxIntegerFrom
        {
            get
            {
                return txtIntegerFrom;
            }
        }

        public TextBox TextBoxIntegerTo
        {
            get
            {
                return txtIntegerTo;
            }
        }


        public FieldType Type
        {
            get
            {
                if (string.IsNullOrEmpty(hdnFieldType.Value))
                    return FieldType.Text;

                return (FieldType)Convert.ToInt32(hdnFieldType.Value);
            }
            set
            {
                hdnFieldType.Value = Convert.ToString((int)value);

                if (value == FieldType.Date)
                {
                    divDate.Visible = true;
                    txtText.Visible = false;
                    ddlList.Visible = false;
                    divDateRange.Visible = false;
                    divIntegerRange.Visible = false;
                }
                else if (value == FieldType.List)
                {
                    divDate.Visible = false;
                    txtText.Visible = false;
                    ddlList.Visible = true;
                    divDateRange.Visible = false;
                    divIntegerRange.Visible = false;
                }
                else if (value == FieldType.DateRange)
                {
                    divDate.Visible = false;
                    txtText.Visible = false;
                    ddlList.Visible = false;
                    divDateRange.Visible = true;
                    divIntegerRange.Visible = false;
                }
                else if (value == FieldType.IntegerRange)
                {
                    divDate.Visible = false;
                    txtText.Visible = false;
                    ddlList.Visible = false;
                    divDateRange.Visible = false;
                    divIntegerRange.Visible = true;
                }
            }
        }

        public FilterDataType DataType
        {
            get
            {
                if (string.IsNullOrEmpty(hdnFieldDataType.Value))
                    return FilterDataType.String;

                return (FilterDataType)Convert.ToInt32(hdnFieldDataType.Value);
            }
            set
            {
                hdnFieldDataType.Value = Convert.ToInt32(value).ToString();
            }
        }

        public FilterOperator Operator
        {
            get
            {
                switch (ddlOperator.SelectedValue)
                {
                    case "Equal":
                        return FilterOperator.Equal;
                    case "LessThan":
                        return FilterOperator.LessThan;
                    case "LessThanEqual":
                        return FilterOperator.LessThanEqual;
                    case "MoreThan":
                        return FilterOperator.MoreThan;
                    case "MoreThanEqual":
                        return FilterOperator.MoreThanEqual;
                    case "NotEqual":
                        return FilterOperator.NotEqual;
                    case "Like":
                        return FilterOperator.Like;
                    default:
                        return FilterOperator.Equal;
                }
            }
            set
            {
                ddlOperator.SelectedValue = value.ToString();
            }
        }

        public object CompareValue
        {
            get
            {
                switch (Type)
                {
                    case FieldType.Text:
                        if (DataType == FilterDataType.Integer)
                            return Convert.ToInt32(txtText.Text);
                        else
                            return txtText.Text;
                    case FieldType.Date:
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(txtDate.Text, out dt))
                            return dt;
                        else
                            throw new Exception("Invalid Date Format: " + FieldText);
                    case FieldType.List:
                        if (DataType == FilterDataType.Integer)
                            return Convert.ToInt32(ddlList.SelectedValue);
                        return ddlList.SelectedValue;
                    default:
                        return null;
                }
            }
        }

        public SelectFilter SelectFilter
        {
            get
            {
                return new SelectFilter(hdnField.Value, Operator, CompareValue);
            }
        }

        public List<SelectFilter> DateRangeFilters
        {
            get
            {
                DateTime dtFrom = new DateTime();
                DateTime dtTo = new DateTime();
                
                if (!DateTime.TryParse(txtDateFrom.Text, out dtFrom))
                    throw new Exception("Invalid Date Format: " + FieldText);

                if (!DateTime.TryParse(txtDateTo.Text, out dtTo))
                    throw new Exception("Invalid Date Format: " + FieldText);
                
                var from = new SelectFilter(hdnField.Value, FilterOperator.MoreThanEqual, dtFrom);
                var to = new SelectFilter(hdnField.Value, FilterOperator.LessThanEqual, dtTo);

                List<SelectFilter> selectFilters = new List<SelectFilter>();
                selectFilters.Add(from);
                selectFilters.Add(to);

                return selectFilters;
            }
        }

        public List<SelectFilter> IntegerRangeFilters
        {
            get
            {
                int intFrom = 0;
                int intTo = 0;

                if (!int.TryParse(txtIntegerFrom.Text, out intFrom))
                    throw new Exception("Invalid Integer Format: " + FieldText);

                if (!int.TryParse(txtIntegerTo.Text, out intTo))
                    throw new Exception("Invalid Integer Format: " + FieldText);

                var from = new SelectFilter(hdnField.Value, FilterOperator.MoreThanEqual, intFrom);
                var to = new SelectFilter(hdnField.Value, FilterOperator.LessThanEqual, intTo);

                List<SelectFilter> selectFilters = new List<SelectFilter>();
                selectFilters.Add(from);
                selectFilters.Add(to);

                return selectFilters;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtText.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
                txtDateFrom.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
                txtDateTo.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
                txtIntegerFrom.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
                txtIntegerTo.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
                ddlList.Attributes.Add("onfocus", "check(" + chkSelected.ClientID + ")");
            }
        }
    }

    public enum FieldType { Text = 1, Date = 2, List = 3, DateRange = 4, IntegerRange = 5 }
}