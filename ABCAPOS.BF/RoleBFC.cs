using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class RoleBFC : GenericBFC<Role, Role, RoleModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Role, RoleModel> GetDAC()
        {
            return new GenericDAC<Role, RoleModel>("ID", false, "Name");
        }

        protected override GenericDAC<Role, RoleModel> GetViewDAC()
        {
            return new GenericDAC<Role, RoleModel>("ID", false, "Name");
        }

        public void Create(RoleModel role, List<RoleDetailModel> roleDetails, string userName)
        {
            var dac = new ABCAPOSDAC();

            role.CreatedBy = role.ModifiedBy = userName;
            role.CreatedDate = role.ModifiedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateRole(role);
                dac.CreateRoleDetails(role.ID, role.Details);

                trans.Complete();
            }
        }

        public void Update(RoleModel role, List<RoleDetailModel> roleDetails, string userName)
        {
            var dac = new ABCAPOSDAC();

            var extObj = RetrieveByID(role.ID);

            extObj.Name = role.Name;
            extObj.IsActive = role.IsActive;
            extObj.ModifiedBy = userName;
            extObj.ModifiedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(extObj);
                dac.DeleteRoleDetails(role.ID);
                dac.CreateRoleDetails(role.ID, role.Details);

                trans.Complete();
            }
        }

        public override void DeleteByID(object id)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                base.DeleteByID(id);
                dac.DeleteRoleDetails(Convert.ToInt32(id));

                trans.Complete();
            }
        }

        public List<RoleModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveRole(isActive);
        }

        public List<RoleModel> RetreiveRoleAdminstrator(bool isActive)
        {
            return new ABCAPOSDAC().RetreiveRoleAdministrator(isActive);
        }

        public List<RoleModel> RetreiveListRoleAdministrator(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetreiveListRoleAdministrator(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<RoleModel> RetreiveListRoleRoot(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetreiveListRoleRoot(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<RoleDetailModel> RetrieveDetails(int roleID)
        {
            return new ABCAPOSDAC().RetrieveRoleDetails(roleID);
        }

        public List<RoleDetailModel> RetrieveActions(string moduleID)
        {
            return new ABCAPOSDAC().RetrieveRoleActions(moduleID);
        }

        public List<string> RetrieveActions(int roleID, string moduleID)
        {
            return new ABCAPOSDAC().RetrieveRoleActions(roleID, moduleID);
        }

        public bool CheckIsAllowed(int roleID, string moduleID, string action)
        {
            List<RoleDetailModel> listDetail = this.RetrieveDetails(roleID).Where(p => p.ModuleID == moduleID && p.Action == action).ToList();
            return listDetail.Count > 0;
        }
    }
}
