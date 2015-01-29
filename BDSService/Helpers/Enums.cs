using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BDSService.Helpers
{
    public class Enums
    {
        public enum CustomRestrictionPermission
        {
            None = 0,
            ViewOnly = 1,
            ViewEdit = 2
        }
        public static Dictionary<string, string> CustomRestrictionValues = new Dictionary<string, string>(){
        { CustomRestrictionPermission.None.ToString(),"None"},
            { CustomRestrictionPermission.ViewOnly.ToString(),"View Only"},
            {CustomRestrictionPermission.ViewEdit.ToString(),"View/Edit"}
         };
        
        public enum ActivityType
        {
            User,
            Client
        }
    }
}