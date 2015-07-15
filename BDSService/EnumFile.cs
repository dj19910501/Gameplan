using System.Collections.Generic;

/*
 *  Author: Maninder Singh Wadhva
 *  Created Date: 11/27/2013
 *  Purpose: Holds enum for BDSAuth.
 */

namespace BDSService
{
    public class EnumFile
    {
        /// <summary>
        /// Enum for role.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public enum Role
        {
            SystemAdmin = 0,
            ClientAdmin = 1,
            Director = 2,
            Planner = 3,
        }

        /// <summary>
        /// Data Dictionary to hold role code values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public static Dictionary<string, string> RoleCodeValues = new Dictionary<string, string>()
        {
            {Role.SystemAdmin.ToString(), "SA"},
            {Role.ClientAdmin.ToString(), "CA"},
            {Role.Director.ToString(), "D"},
            {Role.Planner.ToString(), "P"}
        };

        /// <summary>
        /// Enum for role codes.
        /// Added By: Kuber B Joshi.
        /// Date: 12/11/2013
        /// </summary>
        public enum RoleCodes
        {
            SA,
            CA,
            D,
            P
        }
    }
}
