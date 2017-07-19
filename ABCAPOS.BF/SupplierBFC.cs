using MPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.BF
{
    public class SupplierBFC 
    {
    //    : GenericBFC<Supplier, v_Supplier, SupplierModel>
    //{
    //    public override string GenerateID()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetSupplierCode()
    //    {
    //        var prefixSetting = new PrefixSettingBFC().Retrieve();
    //        var supplierPrefix = "";

    //        if (prefixSetting != null)
    //            supplierPrefix = prefixSetting.SupplierPrefix;

    //        var code = new ABCAPOSDAC().RetrieveSupplierMaxCode(supplierPrefix, 4);

    //        return code;
    //    }

    //    protected override GenericDAC<Supplier, SupplierModel> GetDAC()
    //    {
    //        return new GenericDAC<Supplier, SupplierModel>("ID", false, "Name");
    //    }

    //    protected override GenericDAC<v_Supplier, SupplierModel> GetViewDAC()
    //    {
    //        return new GenericDAC<v_Supplier, SupplierModel>("ID", false, "Name");
    //    }

    //    public SupplierModel RetrieveByCode(string supplierCode)
    //    {
    //        return new ABCAPOSDAC().RetrieveSupplierByCode(supplierCode);
    //    }

    //    public List<SupplierModel> RetrieveAutoComplete(string key)
    //    {
    //        return new ABCAPOSDAC().RetrieveSupplierAutoComplete(key);
    //    }
    }
}
