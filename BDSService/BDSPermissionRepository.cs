using BDSService.BDSEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDSService
{
    public class BDSPermissionRepository
    {
        #region Variables

        private BDSAuthEntities db = new BDSAuthEntities();

        #endregion

        /// <summary>
        /// Returns the Roles its permission for the menus and items
        /// </summary>
        /// <param name="ApplicationId"></param>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public List<Permission> GetPermission(Guid ApplicationId, Guid RoleId)
        {
            try
            {
                List<Permission> lstPermission = (from p in db.Role_Permission
                                                  join m in db.Menu_Application on p.MenuApplicationId equals m.MenuApplicationId
                                                  where p.IsDeleted == false && p.RoleId == RoleId && m.IsDeleted == false && m.ApplicationId == ApplicationId
                                                  select new Permission
                                                  {
                                                      Code = m.Code,
                                                      PermissionCode = p.PermissionCode
                                                  }).ToList();
                return lstPermission;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        /// <summary>
        /// returns list of user application permissions
        /// added by dharmraj, ticket #519
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public List<string> GetUserActivityPermission(Guid userId, Guid applicationId)
        {
            List<string> lstUserActivityPermission = (from uap in db.User_Activity_Permission
                                                      join aa in db.Application_Activity on uap.ApplicationActivityId equals aa.ApplicationActivityId
                                                      where uap.UserId == userId && aa.ApplicationId == applicationId
                                                      select aa.Code).ToList();

            return lstUserActivityPermission;
        }
    }
}
