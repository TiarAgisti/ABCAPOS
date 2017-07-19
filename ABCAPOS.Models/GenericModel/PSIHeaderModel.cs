using ABCAPOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models.GenericModel
{
    public abstract class PSIHeaderModel
    {
        /*TABLE AUTOMATIC*/
        public long ID { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string VoidedBy { get; set; }
        public string VoidRemarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime VoidedDate { get; set; }

        /*TABLE MANUAL*/
        [Required]
        public string Code { get; set; }

        /*INIT*/
        public PSIHeaderModel(){
            this.Code = string.IsNullOrEmpty(this.Code) ? "-" : this.Code;
            this.Status = this.ID == 0 ? (int)MPL.DocumentStatus.New : this.Status;
            this.CreatedDate = this.ID == 0 ? SystemConstants.UnsetDateTime : this.CreatedDate;
            this.ModifiedDate = this.ID == 0 ? SystemConstants.UnsetDateTime : this.ModifiedDate;
            this.ApprovedDate = this.ID == 0 ? SystemConstants.UnsetDateTime : this.ApprovedDate;
            this.VoidedDate = this.ID == 0 ? SystemConstants.UnsetDateTime : this.VoidedDate;
        }
    }
}
