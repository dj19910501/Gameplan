using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BDSService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BDSService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BDSService.svc or BDSService.svc.cs at the Solution Explorer and start debugging.
    public class BDSService : IBDSService
    {
        public BDSEntities.User ValidateUser(Guid ApplicationId, string UserEmail, string UserPassword)
        {
            BDSAuthorization obj = new BDSAuthorization();
            return obj.ValidateUser(ApplicationId, UserEmail, UserPassword);
        }
        public BDSEntities.User ValidateUserByClient(Guid ApplicationId, string UserEmail, string UserPassword, Guid ClientId)
        {
            BDSAuthorization obj = new BDSAuthorization();
            return obj.ValidateUser(ApplicationId, UserEmail, UserPassword, ClientId);
        }

        public BDSEntities.User GetTeamMemberDetails(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetTeamMemberDetails(userId, applicationId);
        }

        public List<BDSEntities.User> GetTeamMemberList(Guid clientId, Guid applicationId, Guid userId, bool isSystemAdmin)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetTeamMemberList(clientId, applicationId, userId, isSystemAdmin);
        }

        public List<BDSEntities.Client> GetClientList()
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetClientList();
        }

        public BDSEntities.Client GetClientById(Guid clientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetClientById(clientId);
        }

        //Modified By : Kalpesh Sharma
        //Role logical deletion and Application id in Custom restrication
        //Changes : Currenlty we are not use this method that's why we are commnet it . 
        //public List<BDSEntities.Role> GetRoleList(string roleCodes)
        //{
        //    BDSUserRepository obj = new BDSUserRepository();
        //    return obj.GetRoleList(roleCodes);
        //}

        public List<BDSEntities.User> GetMultipleTeamMemberDetails(string userIdList, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetMultipleTeamMemberDetails(userIdList, applicationId);
        }

        public int ChangePassword(Guid userId, string newPassword, string currPassword)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.ChangePassword(userId, newPassword, currPassword);
        }

        public bool CheckCurrentPassword(Guid userId, string currentPassword)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CheckCurrentPassword(userId, currentPassword);
        }

        public List<BDSEntities.Menu> GetMenu(Guid applicationId, Guid roleId)
        {
            BDSMenuRepository obj = new BDSMenuRepository();
            return obj.GetMenu(applicationId, roleId);
        }
        public List<BDSEntities.Menu> GetAllMenu(Guid applicationId, Guid roleId)
        {
            BDSMenuRepository obj = new BDSMenuRepository();
            return obj.GetAllMenu(applicationId, roleId);
        }

        public List<BDSEntities.Permission> GetPermission(Guid applicationId, Guid roleId)
        {
            BDSPermissionRepository obj = new BDSPermissionRepository();
            return obj.GetPermission(applicationId, roleId);
        }

        public bool CheckEmail(string email)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CheckEmail(email);
        }

        public BDSEntities.User GetUserDetails(string userEmail)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserDetails(userEmail);
        }

        public int DeleteUser(Guid userId,Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.DeleteUser(userId,applicationId);
        }

        public int CreateUserWithPermission(BDSEntities.User user, Guid applicationId, Guid createdBy, string VerticalIds, string GeographyIds, string BusinessUnitIds)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CreateUserWithPermission(user, applicationId, createdBy,VerticalIds,GeographyIds,BusinessUnitIds);
        }

        public int CreateUser(BDSEntities.User user, Guid applicationId, Guid createdBy)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CreateUser(user, applicationId, createdBy);
        }

        public int UpdateUser(BDSEntities.User user, Guid applicationId, Guid modifiedBy)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.UpdateUser(user, applicationId, modifiedBy);
        }

        //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        public string GetUserRole(Guid id, Guid applicationId , Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserRole(id, applicationId, ClientId);
        }

        public string GetApplicationName(Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetApplicationName(applicationId);
        }

        public string GetClientName(Guid userId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetClientName(userId);
        }

        public List<int> AllowedMenusForUser(Guid roleId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.AllowedMenusForUser(roleId);
        }

        public string GetMenuName(int menuId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetMenuName(menuId);
        }

        public int ResetPassword(Guid userId, string SingleHash_NewPassword)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.ResetPassword(userId, SingleHash_NewPassword);
        }

        public BDSEntities.Role GetRoleDetails(Guid roleId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetRoleDetails(roleId);
        }

        public int UpdateLastLoginDate(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.UpdateLastLoginDate(userId, applicationId);
        }

        public BDSEntities.PasswordResetRequest GetPasswordResetRequest(Guid PasswordResetRequestId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetPasswordResetRequest(PasswordResetRequestId);
        }

        public string CreatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CreatePasswordResetRequest(objPasswordResetRequest);
        }

        public int UpdatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.UpdatePasswordResetRequest(objPasswordResetRequest);
        }

        public List<BDSEntities.SecurityQuestion> GetSecurityQuestion()
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetSecurityQuestion();
        }

        public int UpdateUserSecurityQuestion(BDSEntities.User user)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.UpdateUserSecurityQuestion(user);
        }
        public List<BDSEntities.User> GetUserListByClientId(Guid clientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserListByClientId(clientId);
        }

        public string GetApplicationReleaseVersion(Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetApplicationReleaseVersion(applicationId);
        }

        public List<string> GetUserActivityPermission(Guid userId, Guid applicationId)
        {
            BDSPermissionRepository obj = new BDSPermissionRepository();
            return obj.GetUserActivityPermission(userId, applicationId);
        }

        /// added by uday for #513
        public List<BDSEntities.ApplicationActivity> GetUserApplicationactivitylist(Guid applicationid)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserApplicationactivitylist(applicationid);
        }

        /// added by uday for #513
        //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        public int DuplicateRoleCheck(BDSEntities.Role role, Guid applicationId,Guid ClientID)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.DuplicateRoleCheck(role, applicationId, ClientID);
        }

        /// added by uday for #513
        public List<BDSEntities.User> GetRoleMemberList(Guid applicationId, Guid roleid)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetRoleMemberList(applicationId, roleid);
        }

        /// added by uday for #513
        /// Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        public int DeleteRoleAndReassign(Guid delroleid, Guid? reassignroleid, Guid applicationid, Guid modifiedBy , Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.DeleteRoleAndReassign(delroleid, reassignroleid, applicationid, modifiedBy, ClientId);
        }

        /// added by uday for #513
        ///Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        public int CreateRole(string roledesc, string permissionID, string colorcode, Guid applicationid, Guid createdby, Guid roleid, string delpermission , Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CreateRole(roledesc, permissionID, colorcode, applicationid, createdby, roleid, delpermission,ClientId);
        }
        ///Added By : Arpita Soni for Ticket #131 
        public Guid CreateRoleWithoutPermission(string roledesc,string colorcode, Guid applicationid, Guid createdby, Guid roleid, string delpermission, Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CreateRoleWithoutPermission(roledesc, colorcode, applicationid, createdby, roleid, delpermission, ClientId);
        }
        /// added by uday for #513
        public int CopyRole(string copyroledesc, Guid originalid, Guid applicationid, Guid createdby,Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.CopyRole(copyroledesc, originalid, applicationid, createdby, ClientId);
        }

        /// added by uday for #513
        public List<BDSEntities.ApplicationActivity> GetRoleactivitypermissions(Guid roleid)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetRoleactivitypermissions(roleid);
        }

        /// added by uday for #513
        //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        public List<BDSEntities.Role> GetAllRoleList(Guid applicationid , Guid ClientId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetAllRoleList(applicationid, ClientId);
        }

        //public List<BDSEntities.ApplicationActivity> GetAllApplicationActivity(Guid applicationId)
        //{
        //    BDSUserRepository obj = new BDSUserRepository();
        //    return obj.GetAllApplicationActivity(applicationId);
        //}

        public List<BDSEntities.UserApplicationPermission> GetUserActivity(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserActivity(userId, applicationId);
        }

        public List<BDSEntities.CustomRestriction> GetUserCustomRestrictionList(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserCustomRestrictionList(userId, applicationId);
        }

        public int AddUserActivityPermissions(Guid userId, Guid CreatorId, string[] permissions, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.AddUserActivityPermissions(userId, CreatorId, permissions, applicationId);
        }

        public int DeleteUserActivityPermission(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.DeleteUserActivityPermission(userId, applicationId);
        }
        public int DeleteUserCustomrestriction(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.DeleteUserCustomrestriction(userId, applicationId);
        }
        public int resetToRoleDefault(Guid userId, Guid CreatorId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.resetToRoleDefault(userId, CreatorId, applicationId);
        }
        
        //Added by Arpita Soni for Ticket #132
        public Guid GetRoleIdFromUser(Guid userId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetRoleIdFromUser(userId, applicationId);
        }
        
        public List<BDSEntities.UserHierarchy> GetUserHierarchy(Guid clientId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserHierarchy(clientId, applicationId);
        }

        public List<BDSEntities.User> GetManagerList(Guid clientId, Guid applicationId, Guid userId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetManagerList(clientId, applicationId, userId);
        }
        public List<BDSEntities.User> GetOtherApplicationUsers(Guid clientId, Guid applicationId)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetOtherApplicationUsers(clientId, applicationId);
        }
        public int AssignUser(Guid UserId, Guid RoleId, Guid applicationId, Guid createdBy)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.AssignUser(UserId, RoleId, applicationId, createdBy);
        }

        public List<BDSEntities.User> GetUserListWithCustomRestrictions(Guid userId, Guid clientId, Guid applicationId, Dictionary<string, string> customRestrictionFieldIds)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetUserListWithCustomRestrictions(userId, clientId, applicationId, customRestrictionFieldIds);
        }

        public List<BDSEntities.User> GetMultipleTeamMemberName(string userIdList)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetMultipleTeamMemberName(userIdList);
        }
        public List<BDSEntities.ApplicationActivity> GetClientApplicationactivitylist(Guid applicationid)
        {
            BDSUserRepository obj = new BDSUserRepository();
            return obj.GetClientApplicationactivitylist(applicationid);
        }
    }

}
