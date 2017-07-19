using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using System.ComponentModel;
using MPL.Business;

namespace ABCAPOS
{
    public partial class GenericFilter : System.Web.UI.UserControl
    {
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PlaceHolder FilterFields { get; set; }

        public List<SelectFilter> SelectFilters
        {
            get
            {
                List<SelectFilter> selectFilters = new List<SelectFilter>();
                foreach (Control control in phFilterFields.Controls[0].Controls)
                {
                    if (control is GenericFilterField)
                    {
                        var filterField = control as GenericFilterField;

                        if (filterField.Selected)
                        {
                            if (filterField.Type == FieldType.DateRange)
                                selectFilters.AddRange(filterField.DateRangeFilters);
                            else if (filterField.Type == FieldType.IntegerRange)
                                selectFilters.AddRange(filterField.IntegerRangeFilters);
                            else
                                selectFilters.Add(filterField.SelectFilter);
                        }
                    }
                }

                return selectFilters;
            }
        }

        public string ObjectDataSourceID
        {
            get
            {
                return hdnOdsID.Value;
            }
            set 
            { 
                hdnOdsID.Value = value;

            }
        }

        

        public delegate void FilterDelegate();

        [Browsable(true)]
        public event FilterDelegate Filter;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (FilterFields != null)
                phFilterFields.Controls.Add(FilterFields);


            if (!string.IsNullOrEmpty(ObjectDataSourceID))
            {
                var control = this.Parent.FindControl(ObjectDataSourceID);

                if (control != null && control is ObjectDataSource)
                {
                    ObjectDataSource ods = control as ObjectDataSource;                                        
                    ods.Selecting += new ObjectDataSourceSelectingEventHandler(ods_Selecting);
                }
            }
        }

        void ods_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["selectFilters"] = SelectFilters;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                if (Filter != null)
                {
                    Filter();
                }
                lblError.Visible = false;
            }
            catch (Exception ee)
            {
                lblError.Text = ee.Message;
                lblError.Visible = true;

            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            foreach (Control control in phFilterFields.Controls[0].Controls)
            {
                if (control is GenericFilterField)
                {
                    var filterField = control as GenericFilterField;
                    filterField.Selected = false;
                    filterField.TextBoxString.Text = "";
                    filterField.TextBoxDate.Text = "";
                    filterField.TextBoxDateFrom.Text = "";
                    filterField.TextBoxDateTo.Text = "";
                    filterField.TextBoxIntegerFrom.Text = "";
                    filterField.TextBoxIntegerTo.Text = "";

                    if (filterField.DropDownList.Items.Count > 0)
                    {
                        filterField.DropDownList.SelectedIndex = 0;
                    }
                }
            }

            if (Filter != null)
            {
                Filter();
            }
            lblError.Visible = false;
        }
    }
}