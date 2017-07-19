using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using ABCAPOS.DA;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.POIFS.FileSystem;
using NPOI.HPSF;
using System.EnterpriseServices;
using System.Configuration;
using System.Transactions;
using ABCAPOS.Helpers;

namespace ABCAPOS
{
    public partial class CheckEmail : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                this.CheckEmailTest();
        }
        private void CheckEmailTest()
        {
            new EmailHelper().SendSOEmail("hkasmara@abca-indonesia.com", "[CHECK EMAIL ABCA]", "");
        }
        

    }
}

