using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace ABCAPOS
{
    public class ProfileCommon : ProfileBase
    {
        public static ProfileCommon GetProfile(string username)
        {
            var profileCommon = ProfileBase.Create(username);

            var accountProfile = new ProfileCommon();
            accountProfile.Initialize(profileCommon.UserName, true);

            return accountProfile;
        }

        public virtual int RoleID
        {
            get
            {
                return ((int)(this.GetPropertyValue("RoleID")));
            }
            set
            {
                this.SetPropertyValue("RoleID", value);
                this.Save();
            }
        }

        public virtual bool IsActive
        {
            get
            {
                return ((bool)(this.GetPropertyValue("IsActive")));
            }
            set
            {
                this.SetPropertyValue("IsActive", value);
                this.Save();
            }
        }
    }
}