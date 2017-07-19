using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ABCAPOS.BF;

namespace ABCAPOS.Helpers
{
    public class MenuHelper
    {
        public static List<string> GetMenuList()
        {
            var menuList = new List<string>();
            var userName = MembershipHelper.GetUserName();

            if (HttpContext.Current.Session[userName] != null)
                menuList = HttpContext.Current.Session[userName] as List<string>;
            else
            {
                menuList = GetMenu(userName);
                HttpContext.Current.Session[userName] = menuList;
            }

            return menuList;
        }

        public static void ResetMenu(string userName)
        {
            if (HttpContext.Current.Session[userName] != null)
                HttpContext.Current.Session[userName] = null;
        }

        private static List<string> GetMenu(string userName)
        {
            var profile = ProfileCommon.GetProfile(userName);
            var roleMenuList = new RoleBFC().RetrieveDetails(profile.RoleID).Select(p => p.ModuleID).Distinct();

            return roleMenuList.ToList();
        }
    }
}