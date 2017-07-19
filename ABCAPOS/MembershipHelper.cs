using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ABCAPOS
{
    public class MembershipHelper
    {
        public static string GetUserName()
        {
            if (Membership.GetUser() == null)
                return "System";
            else
                return Membership.GetUser().UserName;
        }

        public static int GetRoleID()
        {
            var profile = ProfileCommon.GetProfile(GetUserName());

            return profile.RoleID;
        }

        public static int GetRoleID(string userName)
        {
            var profile = ProfileCommon.GetProfile(userName);

            return profile.RoleID;
        }

        public static List<Models.UserModel> GetAllUsers()
        {
            var returnList = new List<Models.UserModel>();
            var users = Membership.GetAllUsers();
            foreach (MembershipUser user in users)
            {
                var usr = new Models.UserModel();
                usr.UserID = user.UserName;
                returnList.Add(usr);
            }
            return returnList;
        }
    }
}