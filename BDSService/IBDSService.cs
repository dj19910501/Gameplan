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
        List<BDSEntities.Role> GetRoleList(string roleCodes);

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
        int DeleteUser(Guid userId);

        [OperationContract]
        int CreateUser(BDSEntities.User user, Guid applicationId, Guid createdBy);

        [OperationContract]
        int UpdateUser(BDSEntities.User user, Guid applicationId, Guid modifiedBy);

        [OperationContract]
        string GetUserRole(Guid id, Guid applicationId);

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
    }
}
