using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BDSService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBDSService" in both code and config file together.
    [ServiceContract]
    public interface IBDSService
    {
        [OperationContract]
        BDSEntities.User ValidateUser(Guid applicationId, string userEmail, string userPassword);

        [OperationContract]
        BDSEntities.User GetTeamMemberDetails(Guid userId, Guid applicationId);

        [OperationContract]
        List<BDSEntities.User> GetTeamMemberList(Guid clientId, Guid applicationId, Guid userId, bool isSystemAdmin);

        [OperationContract]
        List<BDSEntities.Client> GetClientList();

        [OperationContract]
        BDSEntities.Client GetClientById(Guid clientId);

        //Modified By : Kalpesh Sharma
        //Role logical deletion and Application id in Custom restrication
        //Changes : Currenlty we are not use this method that's why we are commnet it .
        //[OperationContract]
        //List<BDSEntities.Role> GetRoleList(string roleCodes);

        [OperationContract]
        List<BDSEntities.User> GetMultipleTeamMemberDetails(string userIdList, Guid applicationId);

        [OperationContract]
        int ChangePassword(Guid userId, string newPassword, string currPassword);

        [OperationContract]
        bool CheckCurrentPassword(Guid userId, string currentPassword);

        [OperationContract]
        List<BDSEntities.Menu> GetMenu(Guid ApplicationId, Guid RoleId);

        [OperationContract]
        List<BDSEntities.Menu> GetAllMenu(Guid ApplicationId, Guid RoleId);

        [OperationContract]
        List<BDSEntities.Permission> GetPermission(Guid ApplicationId, Guid RoleId);

        [OperationContract]
        bool CheckEmail(string email);

        [OperationContract]
        int DeleteUser(Guid userId,Guid applicationId);

        [OperationContract]
        BDSEntities.User GetUserDetails(string userEmail);

        [OperationContract]
        int CreateUser(BDSEntities.User user, Guid applicationId, Guid createdBy);

        [OperationContract]
        int CreateUserWithPermission(BDSEntities.User user, Guid applicationId, Guid createdBy, string VerticalIds, string GeographyIds, string BusinessUnitIds);

        [OperationContract]
        int UpdateUser(BDSEntities.User user, Guid applicationId, Guid modifiedBy);

        //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        [OperationContract]
        string GetUserRole(Guid id, Guid applicationId,Guid ClientId);

        [OperationContract]
        string GetApplicationName(Guid applicationId);

        [OperationContract]
        string GetClientName(Guid userId);

        [OperationContract]
        List<int> AllowedMenusForUser(Guid roleId);

        [OperationContract]
        string GetMenuName(int menuId);

        [OperationContract]
        int ResetPassword(Guid userId, string SingleHash_NewPassword);

        [OperationContract]
        BDSEntities.Role GetRoleDetails(Guid roleId);

        [OperationContract]
        int UpdateLastLoginDate(Guid userId, Guid applicationId);

        [OperationContract]
        string CreatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest);

        [OperationContract]
        BDSEntities.PasswordResetRequest GetPasswordResetRequest(Guid PasswordResetRequestId);

        [OperationContract]
        int UpdatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest);

        [OperationContract]
        List<BDSEntities.SecurityQuestion> GetSecurityQuestion();

        [OperationContract]
        int UpdateUserSecurityQuestion(BDSEntities.User user);

        [OperationContract]
        List<BDSEntities.User> GetUserListByClientId(Guid clientId);

        [OperationContract]
        string GetApplicationReleaseVersion(Guid applicationId);

        [OperationContract]
        List<string> GetUserActivityPermission(Guid userId, Guid applicationId);

        /// added by uday for #513
        ///Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        [OperationContract]
        List<BDSEntities.Role> GetAllRoleList(Guid applicationid , Guid ClientId);

        /// added by uday for #513
        [OperationContract]
        List<BDSEntities.ApplicationActivity> GetUserApplicationactivitylist(Guid applicationid);

        /// added by uday for #513
        [OperationContract]
        int DuplicateRoleCheck(BDSEntities.Role role, Guid applicationid,Guid ClientID);

        /// added by uday for #513
        [OperationContract]
        List<BDSEntities.User> GetRoleMemberList(Guid applicationId, Guid roleid);

        /// added by uday for #513
        ///Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        [OperationContract]
        int DeleteRoleAndReassign(Guid delroleid, Guid? reassignroleid, Guid applicationid, Guid modifiedBy,Guid ClientId);

        /// added by uday for #513
        //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
        [OperationContract]
        int CreateRole(string roledesc, string permissionID, string colorcode, Guid applicationid, Guid createdby, Guid roleid, string delpermission,Guid ClientId);

        /// added by uday for #513
        [OperationContract]
        int CopyRole(string copyroledesc, Guid originalid, Guid applicationid, Guid createdby , Guid ClientId);

        /// added by uday for #513
        [OperationContract]
        List<BDSEntities.ApplicationActivity> GetRoleactivitypermissions(Guid roleid);

        ///* Added by Mitesh Vaishnav #521 */
        //[OperationContract]
        //List<BDSEntities.ApplicationActivity> GetAllApplicationActivity(Guid applicationId);

        [OperationContract]
        List<BDSEntities.UserApplicationPermission> GetUserActivity(Guid userId, Guid applicationId);

        [OperationContract]
        List<BDSEntities.CustomRestriction> GetUserCustomRestrictionList(Guid userId, Guid applicationId);

        [OperationContract]
        int AddUserActivityPermissions(Guid userId, Guid CreatorId, string[] permissions, Guid applicationId);

        [OperationContract]
        int DeleteUserActivityPermission(Guid userId, Guid applicationId);

        [OperationContract]
        int DeleteUserCustomrestriction(Guid userId, Guid applicationId);

        [OperationContract]
        int resetToRoleDefault(Guid userId, Guid CretorId, Guid applicationId);

        /*End: Added by Mitesh Vaishnav #521 */

        [OperationContract]
        List<BDSEntities.UserHierarchy> GetUserHierarchy(Guid clientId, Guid applicationId);

        [OperationContract]
        List<BDSEntities.User> GetManagerList(Guid clientId, Guid applicationId, Guid userId);

        [OperationContract]
        List<BDSEntities.User> GetOtherApplicationUsers(Guid clientId, Guid applicationId);

        [OperationContract]
        int AssignUser(Guid UserId, Guid RoleId, Guid applicationId, Guid createdBy);

        [OperationContract]
        List<BDSEntities.User> GetUserListWithCustomRestrictions(Guid userId, Guid clientId, Guid applicationId, Dictionary<string, string> customRestrictionFieldIds);
        
        [OperationContract]
        List<BDSEntities.User> GetMultipleTeamMemberName(string userIdList);

        [OperationContract]
        List<BDSEntities.ApplicationActivity> GetClientApplicationactivitylist(Guid applicationid);

    }
}
