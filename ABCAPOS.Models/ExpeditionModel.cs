using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Models.GenericModel;

namespace ABCAPOS.Models
{
    public class ExpeditionModel:PSIHeaderModel
    {
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        public Boolean IsActive { get; set; }

        //Read Only
        public string IsActiveDescription { get; set; }
        //Read Only

        public List<ExpeditionDetailModel> Details { get; set; }
    }
}
