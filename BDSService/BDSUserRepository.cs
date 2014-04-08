using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Elmah;
using System.IO;

using System.Web;
using BDSService.Helpers;

namespace BDSService
{
    public class BDSUserRepository
    {
        #region Variables

        private BDSAuthEntities db = new BDSAuthEntities();

        #endregion

        #region Get Team Member List

        /// <summary>
        /// Function to get team member list for specific client & application.
        /// </summary>
        /// <param name="clientId">client</param>
        /// <param name="applicationId">application</param>
        /// <param name="userId">user</param>
        /// <param name="isSystemAdmin">whether user is admin of or not</param>
        /// <returns>Returns team member list for specific client & application.</returns>
        public List<BDSEntities.User> GetTeamMemberList(Guid clientId, Guid applicationId, Guid userId, bool isSystemAdmin)
        {
            List<BDSEntities.User> teamMemberList = new List<BDSEntities.User>();
            List<User> lstUser = (from u in db.Users
                                  join ua in db.User_Application on u.UserId equals ua.UserId
                                  where u.ClientId == clientId && ua.ApplicationId == applicationId && u.IsDeleted == false && u.UserId != userId
                                  select u).OrderBy(q => q.FirstName).ToList();
            if (lstUser.Count > 0)
            {
                foreach (var user in lstUser)
                {
                    BDSEntities.User userEntity = new BDSEntities.User();
                    userEntity.UserId = user.UserId;
                    userEntity.ClientId = user.ClientId;
                    userEntity.BusinessUnitId = user.BusinessUnitId;
                    userEntity.GeographyId = user.GeographyId;
                    userEntity.Client = db.Clients.Where(cl => cl.ClientId == user.ClientId).Select(c => c.Name).FirstOrDefault();
                    userEntity.DisplayName = user.DisplayName;
                    userEntity.Email = user.Email;
                    userEntity.FirstName = user.FirstName;
                    userEntity.JobTitle = user.JobTitle;
                    userEntity.LastName = user.LastName;
                    userEntity.Password = user.Password;
                    userEntity.ProfilePhoto = user.ProfilePhoto;
                    userEntity.RoleId = db.User_Application.Where(ua => ua.ApplicationId == applicationId && ua.UserId == user.UserId).Select(u => u.RoleId).FirstOrDefault();
                    userEntity.RoleCode = db.Roles.Where(rl => rl.RoleId == userEntity.RoleId).Select(r => r.Code).FirstOrDefault();
                    userEntity.RoleTitle = db.Roles.Where(rl => rl.RoleId == userEntity.RoleId).Select(r => r.Title).FirstOrDefault();
                    if (!isSystemAdmin)
                    {
                        if (userEntity.RoleCode != null)
                        {
                            EnumFile.Role role = CommonFile.GetKey<EnumFile.Role>(EnumFile.RoleCodeValues, userEntity.RoleCode);
                            if (role != EnumFile.Role.SystemAdmin)
                            {
                                teamMemberList.Add(userEntity);
                            }
                        }
                    }
                    else
                    {
                        teamMemberList.Add(userEntity);
                    }
                }
            }
            return teamMemberList;
        }

        #endregion

        #region Get Client List

        /// <summary>
        /// Function to get client list.
        /// </summary>
        /// <returns>Returns client list.</returns>
        public List<BDSEntities.Client> GetClientList()
        {
            List<BDSEntities.Client> clientList = new List<BDSEntities.Client>();
            List<Client> lstClient = db.Clients.Where(c => c.IsDeleted == false).OrderBy(q => q.Name).ToList();
            if (lstClient.Count > 0)
            {
                foreach (var client in lstClient)
                {
                    BDSEntities.Client clientEntity = new BDSEntities.Client();
                    clientEntity.Address1 = client.Address1;
                    clientEntity.Address2 = client.Address2;
                    clientEntity.City = client.City;
                    clientEntity.ClientId = client.ClientId;
                    clientEntity.Code = client.Code;
                    clientEntity.Country = client.Country;
                    clientEntity.IsDeleted = client.IsDeleted;
                    clientEntity.Logo = Convert.ToString(client.Logo);
                    clientEntity.Name = client.Name;
                    clientEntity.State = client.State;
                    clientEntity.ZipCode = client.ZipCode;
                    clientEntity.CreatedBy = client.CreatedBy;
                    clientEntity.CreatedDate = client.CreatedDate;
                    clientEntity.ModifiedBy = client.ModifiedBy;
                    clientEntity.ModifiedDate = client.ModifiedDate;
                    clientList.Add(clientEntity);
                }
            }
            return clientList;
        }

        #endregion

        #region Get Role List

        /// <summary>
        /// Function to get client list for specific role codes.
        /// </summary>
        /// <param name="roleCodes">comma seperated list of role codes</param>
        /// <returns>Returns client list for sepcific role codes.</returns>
        public List<BDSEntities.Role> GetRoleList(string roleCodes)
        {
            List<BDSEntities.Role> roleList = new List<BDSEntities.Role>();
            List<Role> lstRole = new List<Role>();
            List<string> roles = roleCodes.Split(',').ToList<string>();

            if (roleCodes != string.Empty)
            {
                lstRole = db.Roles.Where(r => roles.Contains(r.Code) && r.IsDeleted == false).OrderBy(q => q.Title).ToList();
            }
            else
            {
                lstRole = db.Roles.Where(r => r.IsDeleted == false).OrderBy(q => q.Title).ToList();
            }

            if (lstRole.Count > 0)
            {
                foreach (var role in lstRole)
                {
                    BDSEntities.Role roleEntity = new BDSEntities.Role();
                    roleEntity.RoleId = role.RoleId;
                    roleEntity.Code = role.Code;
                    roleEntity.Title = role.Title;
                    roleEntity.Description = role.Description;
                    roleEntity.IsDeleted = role.IsDeleted;
                    roleEntity.CreatedBy = role.CreatedBy;
                    roleEntity.CreatedDate = role.CreatedDate;
                    roleEntity.ModifiedBy = role.ModifiedBy;
                    roleEntity.ModifiedDate = role.ModifiedDate;
                    roleList.Add(roleEntity);
                }
            }
            return roleList;
        }

        #endregion

        #region Get Team Member Details

        /// <summary>
        /// Function to get details for specific user & application.
        /// </summary>
        /// <param name="userId">user</param>
        /// <param name="applicationId">application</param>
        /// <returns>Returns details of specific user for specific user & application.</returns>
        public BDSEntities.User GetTeamMemberDetails(Guid userId, Guid applicationId)
        {
            BDSEntities.User userObj = new BDSEntities.User();
            User user = db.Users.Where(usr => usr.UserId == userId).FirstOrDefault();
            if (user != null)
            {
                userObj.UserId = user.UserId;
                userObj.BusinessUnitId = user.BusinessUnitId;
                userObj.GeographyId = user.GeographyId;
                userObj.ClientId = user.ClientId;
                userObj.Client = db.Clients.Where(cl => cl.ClientId == user.ClientId).Select(c => c.Name).FirstOrDefault();
                userObj.DisplayName = user.DisplayName;
                userObj.Email = user.Email;
                userObj.FirstName = user.FirstName;
                userObj.JobTitle = user.JobTitle;
                userObj.LastName = user.LastName;
                userObj.Password = user.Password;
                userObj.ProfilePhoto = user.ProfilePhoto;
                userObj.RoleId = db.User_Application.Where(ua => ua.ApplicationId == applicationId && ua.UserId == user.UserId).Select(u => u.RoleId).FirstOrDefault();
                userObj.RoleCode = db.Roles.Where(rl => rl.RoleId == userObj.RoleId).Select(r => r.Code).FirstOrDefault();
                userObj.RoleTitle = db.Roles.Where(rl => rl.RoleId == userObj.RoleId).Select(r => r.Title).FirstOrDefault();
                userObj.SecurityQuestionId = user.SecurityQuestionId;
                userObj.SecurityQuestion = db.SecurityQuestions.Where(sq => sq.SecurityQuestionId == user.SecurityQuestionId).Select(s => s.SecurityQuestion1).FirstOrDefault();
                userObj.Answer = user.Answer;
            }
            return userObj;
        }

        #endregion

        #region Get Multiple Team Member Details

        /// <summary>
        /// Function to get details for list for specific users.
        /// </summary>
        /// <param name="userIdList">comma seperated list of users</param>
        /// <param name="applicationId">application</param>
        /// <returns>Returns client list for sepcific users.</returns>
        public List<BDSEntities.User> GetMultipleTeamMemberDetails(string userIdList, Guid applicationId)
        {
            List<BDSEntities.User> teamMemberList = new List<BDSEntities.User>();
            if (!string.IsNullOrWhiteSpace(userIdList))
            {
                string[] arrUser = userIdList.Split(',');
                if (arrUser.Count() > 0)
                {
                    foreach (var usr in arrUser)
                    {
                        Guid userId = Guid.Parse(usr.ToString());
                        User user = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                        BDSEntities.User userObj = new BDSEntities.User();
                        if (user != null)
                        {
                            userObj.UserId = user.UserId;
                            userObj.BusinessUnitId = user.BusinessUnitId;
                            userObj.GeographyId = user.GeographyId;
                            userObj.ClientId = user.ClientId;
                            userObj.Client = db.Clients.Where(cl => cl.ClientId == user.ClientId).Select(c => c.Name).FirstOrDefault();
                            userObj.DisplayName = user.DisplayName;
                            userObj.Email = user.Email;
                            userObj.FirstName = user.FirstName;
                            userObj.JobTitle = user.JobTitle;
                            userObj.LastName = user.LastName;
                            userObj.Password = user.Password;
                            userObj.ProfilePhoto = user.ProfilePhoto;
                            userObj.RoleId = db.User_Application.Where(ua => ua.ApplicationId == applicationId && ua.UserId == user.UserId).Select(u => u.RoleId).FirstOrDefault();
                            userObj.RoleCode = db.Roles.Where(rl => rl.RoleId == userObj.RoleId).Select(r => r.Code).FirstOrDefault();
                            userObj.RoleTitle = db.Roles.Where(rl => rl.RoleId == userObj.RoleId).Select(r => r.Title).FirstOrDefault();
                            teamMemberList.Add(userObj);
                        }
                    }
                }
            }
            return teamMemberList;
        }

        #endregion

        #region Change Password

        /// <summary>
        /// Function to update the password for specific user.
        /// </summary>
        /// <param name="userId">user</param>
        /// <param name="newPassword">new password</param>
        /// <param name="currPassword">old password</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int ChangePassword(Guid userId, string newPassword, string currPassword)
        {
            int retVal = 0;
            try
            {
                if (userId != null && newPassword != string.Empty && currPassword != string.Empty)
                {
                    byte[] saltbytes = Common.GetSaltBytes();

                    string Hash_CurrentPassword = Common.ComputeFinalHash(currPassword, saltbytes);


                    var objCurrPwdCheck = db.Users.Where(u => u.Password == Hash_CurrentPassword && u.IsDeleted == false && u.UserId == userId).FirstOrDefault();
                    if (objCurrPwdCheck == null)
                    {
                        retVal = -1;
                    }
                    else
                    {
                        string Hash_NewPassword = Common.ComputeFinalHash(newPassword, saltbytes);

                        BDSEntities.User userObj = new BDSEntities.User();
                        User user = db.Users.Where(usr => usr.UserId == userId).FirstOrDefault();
                        if (user != null)
                        {
                            user.Password = Hash_NewPassword;
                            db.Entry(user).State = EntityState.Modified;
                            db.SaveChanges();
                            retVal = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Reset Password

        /// <summary>
        /// Function to reset the password for specific user.
        /// </summary>
        /// <param name="userId">user</param>
        /// <param name="SingleHash_NewPassword">hashed new password</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int ResetPassword(Guid userId, string SingleHash_NewPassword)
        {
            int retVal = 0;
            try
            {
                if (userId != null)
                {
                    byte[] saltbytes = Common.GetSaltBytes();
                    string FinalHash_NewPassword = Common.ComputeFinalHash(SingleHash_NewPassword, saltbytes);

                    BDSEntities.User userObj = new BDSEntities.User();
                    User user = db.Users.Where(usr => usr.UserId == userId && usr.IsDeleted == false).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = FinalHash_NewPassword;
                        db.Entry(user).State = EntityState.Modified;
                        var res = db.SaveChanges();
                        retVal = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Check Current Password

        /// <summary>
        /// Function to verify users current password.
        /// </summary>
        /// <param name="userId">user</param>
        /// <param name="currentPassword">current password</param>
        /// <returns>Returns true if the operation is successful, 0 otherwise.</returns>
        public bool CheckCurrentPassword(Guid userId, string currentPassword)
        {
            bool isValid = false;
            try
            {
                if (userId != null && currentPassword != string.Empty)
                {
                    byte[] saltbytes = Common.GetSaltBytes();

                    string FinalHash_Password = Common.ComputeFinalHash(currentPassword, saltbytes);

                    User user = db.Users.Where(u => u.UserId == userId && u.Password == FinalHash_Password && u.IsDeleted == false).SingleOrDefault();
                    if (user != null)
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return isValid;
        }

        #endregion

        #region Check Email

        /// <summary>
        /// Function to check whether the email exists or not.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>Returns true if the operation is successful, 0 otherwise.</returns>
        public bool CheckEmail(string email)
        {
            bool isValid = false;
            try
            {
                if (email != string.Empty)
                {
                    User user = db.Users.Where(u => u.Email == email.Trim() && u.IsDeleted == false).FirstOrDefault();
                    if (user == null)
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return isValid;
        }

        #endregion

        #region Get User Details

        /// <summary>
        /// Function to get details for specific user.
        /// </summary>
        /// <param name="userEmail">userEmail</param>
        /// <returns>Returns details of specific user.</returns>
        public BDSEntities.User GetUserDetails(string userEmail)
        {
            User user = db.Users.FirstOrDefault(varU => varU.Email == userEmail && varU.IsDeleted == false);
            if (user != null)
            {
                BDSEntities.User userObj = new BDSEntities.User();

                userObj.UserId = user.UserId;
                userObj.BusinessUnitId = user.BusinessUnitId;
                userObj.GeographyId = user.GeographyId;
                userObj.ClientId = user.ClientId;
                userObj.DisplayName = user.DisplayName;
                userObj.Email = user.Email;
                userObj.FirstName = user.FirstName;
                userObj.JobTitle = user.JobTitle;
                userObj.LastName = user.LastName;
                userObj.Password = user.Password;
                userObj.ProfilePhoto = user.ProfilePhoto;
                userObj.SecurityQuestionId = user.SecurityQuestionId;
                userObj.Answer = user.Answer;

                return userObj;
            }
            else
                return null;

        }

        #endregion

        #region Delete User

        /// <summary>
        /// Function to delete the user logically.
        /// </summary>
        /// <param name="userId">user</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int DeleteUser(Guid userId)
        {
            int retVal = 0;
            try
            {
                User user = db.Users.Where(u => u.IsDeleted == false && u.UserId == userId).SingleOrDefault();
                user.IsDeleted = true;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                retVal = 1;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Add User

        /// <summary>
        /// Function to insert new user.
        /// </summary>
        /// <param name="user">user entity</param>
        /// <param name="applicationId">application</param>
        /// <param name="createdBy">created by this user</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int CreateUser(BDSEntities.User user, Guid applicationId, Guid createdBy)
        {
            int retVal = 0;
            try
            {
                var objDuplicateCheck = db.Users.Where(u => u.Email.Trim().ToLower() == user.Email.Trim().ToLower() && u.IsDeleted == false).FirstOrDefault();
                if (objDuplicateCheck != null)
                {
                    retVal = -1;
                }
                else
                {
                    User obj = new User();

                    Guid NewUserId = Guid.NewGuid();
                    obj.UserId = NewUserId;

                    byte[] saltbytes = Common.GetSaltBytes();
                    obj.FirstName = user.FirstName;
                    obj.LastName = user.LastName;

                    // Generate final hash i.e. to be stored in DB
                    string FinalPwd = Common.ComputeFinalHash(user.Password, saltbytes);

                    obj.Password = FinalPwd;
                    obj.Email = user.Email;
                    obj.JobTitle = user.JobTitle;
                    obj.ClientId = user.ClientId;
                    if (user.ProfilePhoto != null)
                        obj.ProfilePhoto = user.ProfilePhoto;
                    obj.BusinessUnitId = user.BusinessUnitId;
                    obj.GeographyId = user.GeographyId;
                    obj.CreatedDate = DateTime.Now;
                    obj.IsDeleted = false;
                    obj.CreatedDate = DateTime.Now;
                    db.Entry(obj).State = EntityState.Added;
                    db.Users.Add(obj);
                    int res = db.SaveChanges();

                    //Insert in User_Application
                    User_Application objUser_Application = new User_Application();
                    objUser_Application.UserId = obj.UserId;
                    objUser_Application.ApplicationId = applicationId;
                    objUser_Application.RoleId = user.RoleId;
                    objUser_Application.CreatedDate = DateTime.Now;
                    objUser_Application.CreatedBy = createdBy;
                    db.Entry(objUser_Application).State = EntityState.Added;
                    db.User_Application.Add(objUser_Application);
                    res = db.SaveChanges();

                    retVal = 1;
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Edit User

        /// <summary>
        /// Function to update existing user.
        /// </summary>
        /// <param name="user">user entity</param>
        /// <param name="applicationId">application</param>
        /// <param name="modifiedBy">modified by this user</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int UpdateUser(BDSEntities.User user, Guid applicationId, Guid modifiedBy)
        {
            int retVal = 0;
            try
            {
                var obj = db.Users.Where(u => u.UserId == user.UserId && u.IsDeleted == false).FirstOrDefault();
                if (obj == null)
                {
                    retVal = -1;
                }
                else
                {
                    var objUserEmail = db.Users.Where(u => u.Email == user.Email && u.UserId != user.UserId && u.IsDeleted == false).FirstOrDefault();
                    if (objUserEmail != null)
                    {
                        retVal = -2;
                    }
                    else
                    {
                        obj.UserId = user.UserId;
                        obj.FirstName = user.FirstName;
                        obj.LastName = user.LastName;
                        obj.Email = user.Email;
                        obj.JobTitle = user.JobTitle;
                        obj.ClientId = user.ClientId;
                        obj.BusinessUnitId = user.BusinessUnitId;
                        obj.GeographyId = user.GeographyId;
                        if (user.ProfilePhoto != null)
                        {
                            obj.ProfilePhoto = user.ProfilePhoto;
                        }
                        else
                        {
                            obj.ProfilePhoto = null;
                        }
                        obj.IsDeleted = user.IsDeleted;
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();

                        //Update in User_Application
                        var objUser_Application = db.User_Application.Where(u => u.UserId == user.UserId && u.ApplicationId == applicationId && u.IsDeleted == false).FirstOrDefault();
                        objUser_Application.UserId = user.UserId;
                        objUser_Application.ApplicationId = applicationId;
                        objUser_Application.RoleId = user.RoleId;
                        objUser_Application.ModifiedDate = DateTime.Now;
                        objUser_Application.ModifiedBy = modifiedBy;
                        db.Entry(objUser_Application).State = EntityState.Modified;
                        db.SaveChanges();

                        retVal = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Get User Role

        /// <summary>
        /// Function to get user role code for specific application.
        /// </summary>
        /// <param name="id">user entity</param>
        /// <param name="applicationId">application</param>
        /// <returns>Returns user role code for specific application.</returns>
        public string GetUserRole(Guid id, Guid applicationId)
        {
            string userRoleCode = string.Empty;
            if (id != null && applicationId != null)
            {
                userRoleCode = (from r in db.Roles
                                join ua in db.User_Application on r.RoleId equals ua.RoleId
                                where ua.UserId == id && ua.ApplicationId == applicationId
                                select (r.Code)).FirstOrDefault();
            }
            return userRoleCode;
        }

        #endregion

        #region Get Application Name

        /// <summary>
        /// Function to get application name for specific application.
        /// </summary>
        /// <param name="applicationId">application</param>
        /// <returns>Returns application name for specific application.</returns>
        public string GetApplicationName(Guid applicationId)
        {
            string appName = string.Empty;
            if (applicationId != null)
            {
                appName = (from a in db.Applications
                           where a.ApplicationId == applicationId
                           select (a.Name)).FirstOrDefault();
            }
            return appName;
        }

        #endregion

        #region Get Client Name

        /// <summary>
        /// Function to get client name for name for specific user.
        /// </summary>
        /// <param name="userId">user</param>
        /// <returns>Returns client name for name for specific user.</returns>
        public string GetClientName(Guid userId)
        {
            string clientName = string.Empty;
            if (userId != null)
            {
                clientName = (from u in db.Users
                              where u.UserId == userId && u.IsDeleted == false
                              select (u.Client.Name)).FirstOrDefault();
            }
            return clientName;
        }

        #endregion

        #region Allowed Menus For Role

        /// <summary>
        /// Function to get menu list for specific role.
        /// </summary>
        /// <param name="roleId">role</param>
        /// <returns>Returns menu list for specific role.</returns>
        public List<int> AllowedMenusForUser(Guid roleId)
        {
            List<int> lstMenuId = new List<int>();
            if (roleId != null)
            {
                lstMenuId = (from rp in db.Role_Permission
                             where rp.RoleId == roleId && rp.IsDeleted == false
                             select (rp.MenuApplicationId)).ToList();
            }
            return lstMenuId;
        }

        #endregion

        #region Get Menu Name

        /// <summary>
        /// Function to get menu name for specific menu.
        /// </summary>
        /// <param name="menuId">menu</param>
        /// <returns>Returns menu name for specific menu.</returns>
        public string GetMenuName(int menuId)
        {
            string menuName = string.Empty;
            if (menuId > 0)
            {
                menuName = (from m in db.Menu_Application
                            where m.MenuApplicationId == menuId && m.IsDeleted == false
                            select (m.Name)).FirstOrDefault();
            }
            return menuName;
        }

        #endregion

        #region Get Role Details

        /// <summary>
        /// Function to get role details name for specific role.
        /// </summary>
        /// <param name="roleId">role</param>
        /// <returns>Returns role details name for specific role.</returns>
        public BDSEntities.Role GetRoleDetails(Guid roleId)
        {
            BDSEntities.Role roleObj = new BDSEntities.Role();
            Role role = db.Roles.Where(r => r.RoleId == roleId).FirstOrDefault();
            if (role != null)
            {
                roleObj.Code = role.Code;
                roleObj.Description = role.Description;
                roleObj.Title = role.Title;
            }
            return roleObj;
        }

        #endregion

        #region Update Last Login Date

        /// <summary>
        /// Function to update last login date for user.
        /// </summary>
        /// <param name="userId">user</param>
        /// <param name="applicationId">application</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int UpdateLastLoginDate(Guid userId, Guid applicationId)
        {
            int retVal = 0;
            try
            {
                var obj = db.User_Application.Where(ua => ua.UserId == userId && ua.ApplicationId == applicationId && ua.IsDeleted == false).FirstOrDefault();
                if (obj == null)
                {
                    retVal = -1;
                }
                else
                {
                    obj.LastLoginDate = DateTime.Now;
                    obj.ModifiedBy = userId;
                    obj.ModifiedDate = DateTime.Now;
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    retVal = 1;
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Get Password reset request
        
        /// <summary>
        /// Function to get PasswordResetRequest detail.
        /// </summary>
        /// <param name="PasswordResetRequestId"></param>
        /// <returns>PasswordResetRequest details.</returns>
        public BDSEntities.PasswordResetRequest GetPasswordResetRequest(Guid PasswordResetRequestId)
        {
            BDSEntities.PasswordResetRequest objPasswordResetRequest = new BDSEntities.PasswordResetRequest();

            var obj = db.PasswordResetRequests.FirstOrDefault(varP => varP.PasswordResetRequestId == PasswordResetRequestId);
            if (obj != null)
            {
                objPasswordResetRequest.PasswordResetRequestId = obj.PasswordResetRequestId;
                objPasswordResetRequest.UserId = obj.UserId;
                objPasswordResetRequest.IsUsed = obj.IsUsed;
                objPasswordResetRequest.AttemptCount = obj.AttemptCount;
                objPasswordResetRequest.CreatedDate = obj.CreatedDate;

                return objPasswordResetRequest;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Add Password Reset Request

        /// <summary>
        /// Function to insert password reset request.
        /// </summary>
        /// <param name="objPasswordResetRequest"></param>
        /// <returns>Return PasswordResetRequestId if the operation is successful, empty string otherwise.</returns>
        public string CreatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest)
        {
            string retVal = string.Empty;

            try
            {
                PasswordResetRequest obj = new PasswordResetRequest();
                obj.PasswordResetRequestId = objPasswordResetRequest.PasswordResetRequestId;
                obj.UserId = objPasswordResetRequest.UserId;
                obj.AttemptCount = objPasswordResetRequest.AttemptCount;
                obj.CreatedDate = objPasswordResetRequest.CreatedDate;

                db.PasswordResetRequests.Add(obj);
                db.SaveChanges();

                retVal = obj.PasswordResetRequestId.ToString();

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retVal;
        }

        #region Update Password Reset Request

        /// <summary>
        /// Function to update existing Password Reset Request.
        /// </summary>
        /// <param name="objPasswordResetRequest"></param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int UpdatePasswordResetRequest(BDSEntities.PasswordResetRequest objPasswordResetRequest)
        {
            int retVal = 0;

            try
            {
                var obj = db.PasswordResetRequests.SingleOrDefault(varP => varP.PasswordResetRequestId == objPasswordResetRequest.PasswordResetRequestId);
                if (obj == null)
                {
                    retVal = -1;
                }
                else
                {
                    obj.AttemptCount = objPasswordResetRequest.AttemptCount;
                    obj.IsUsed = objPasswordResetRequest.IsUsed;
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    retVal = 1;
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retVal;
        }

        #endregion

        #endregion

        #region Get Multiple Security Questions
        
        /// <summary>
        /// Function to get list of secutity questions.
        /// </summary>
        /// <returns>Security questions detail.</returns>
        public List<BDSEntities.SecurityQuestion> GetSecurityQuestion()
        {
            List<BDSEntities.SecurityQuestion> lstSecurityQuestion = new List<BDSEntities.SecurityQuestion>();

            var lstObj = db.SecurityQuestions.Where(varP => varP.IsDeleted == false);
            foreach (var obj in lstObj)
            {
                BDSEntities.SecurityQuestion tempObj = new BDSEntities.SecurityQuestion();

                tempObj.SecurityQuestionId = obj.SecurityQuestionId;
                tempObj.SecurityQuestion1 = obj.SecurityQuestion1;

                lstSecurityQuestion.Add(tempObj);
            }

            return lstSecurityQuestion;
        }

        #endregion

        #region Update User security Question and answer

        /// <summary>
        /// Function to update security question and answer existing user.
        /// </summary>
        /// <param name="user">user entity</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int UpdateUserSecurityQuestion(BDSEntities.User user)
        {
            int retVal = 0;
            try
            {
                var obj = db.Users.Where(u => u.UserId == user.UserId && u.IsDeleted == false).FirstOrDefault();
                if (obj == null)
                {
                    retVal = -1;
                }
                else
                {
                    obj.SecurityQuestionId = user.SecurityQuestionId;
                    obj.Answer = user.Answer;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    retVal = 1;

                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retVal;
        }

        #endregion
    }
}
