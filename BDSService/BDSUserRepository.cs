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
using System.Transactions;

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
                    //if (!isSystemAdmin)
                    //{
                    //    if (userEntity.RoleCode != null)
                    //    {
                    //        EnumFile.Role role = CommonFile.GetKey<EnumFile.Role>(EnumFile.RoleCodeValues, userEntity.RoleCode);
                    //        if (role != EnumFile.Role.SystemAdmin)
                    //        {
                    //            teamMemberList.Add(userEntity);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                                teamMemberList.Add(userEntity);
                    //}
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
                userObj.Phone = user.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
                // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                userObj.IsManager = db.User_Application.Where(a => a.IsDeleted.Equals(false) && a.ManagerId == userId && a.ApplicationId == applicationId).Any();
                userObj.ManagerId = db.User_Application.Where(a => a.IsDeleted.Equals(false) && a.UserId == userId && a.ApplicationId == applicationId).Select(a => a.ManagerId).FirstOrDefault();
                if (userObj.ManagerId != null && userObj.ManagerId != Guid.Empty)
                    userObj.ManagerName = db.Users.Where(a => a.IsDeleted.Equals(false) && a.UserId == userObj.ManagerId).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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

        /// <summary>
        /// Function to get list of users of specific client.
        /// </summary>
        /// <param name="clientId">Client Id.</param>
        /// <param name="applicationId">Application Id.</param>
        /// <returns>Returns list of users of specific client.</returns>
        public List<BDSEntities.User> GetUserListByClientId(Guid clientId, Guid applicationId)
        {
            return db.User_Application.Where(user => user.ApplicationId == applicationId && user.User.ClientId == clientId).Select(user =>
                new BDSEntities.User
                {
                    UserId = user.User.UserId,
                    FirstName = user.User.FirstName,
                    LastName = user.User.LastName
                }).ToList();
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
                using (TransactionScope scope = new TransactionScope())     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
                        obj.Phone = user.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
                        objUser_Application.ManagerId = user.ManagerId;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                        db.Entry(objUser_Application).State = EntityState.Added;
                        db.User_Application.Add(objUser_Application);
                        res = db.SaveChanges();

                        // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                        //-- Insert User_Activity_Permission data
                        if (user.RoleId != null && user.RoleId != Guid.Empty)
                        {
                            var lstDefaultRights = db.Role_Activity_Permission.Where(a => a.RoleId == user.RoleId).ToList();

                            if (lstDefaultRights != null)
                            {
                                if (lstDefaultRights.Count > 0)
                                {
                                    foreach (var item in lstDefaultRights)
                                    {
                                        User_Activity_Permission objUser_Activity_Permission = new User_Activity_Permission();
                                        objUser_Activity_Permission.UserId = obj.UserId;
                                        objUser_Activity_Permission.ApplicationActivityId = item.ApplicationActivityId;
                                        objUser_Activity_Permission.CreatedBy = createdBy;
                                        objUser_Activity_Permission.CreatedDate = DateTime.Now;
                                        db.Entry(objUser_Activity_Permission).State = EntityState.Added;
                                        db.User_Activity_Permission.Add(objUser_Activity_Permission);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517

                        retVal = 1;
                    }
                    scope.Complete();       // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
                using (TransactionScope scope = new TransactionScope())     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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
                            obj.Phone = user.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
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

                            // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                            Guid oldRoleId = objUser_Application.RoleId;
                            Guid newRoleId = user.RoleId;
                            // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517

                            objUser_Application.UserId = user.UserId;
                            objUser_Application.ApplicationId = applicationId;
                            objUser_Application.RoleId = user.RoleId;
                            objUser_Application.ModifiedDate = DateTime.Now;
                            objUser_Application.ModifiedBy = modifiedBy;
                            objUser_Application.ManagerId = user.ManagerId;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                            db.Entry(objUser_Application).State = EntityState.Modified;
                            db.SaveChanges();

                            retVal = 1;

                            // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                            //-- When delete a Manager, re-assign the subordinates to other manager.
                            if (user.IsDeleted == true && user.IsManager == true)
                                retVal = ReassignManager(user.UserId, user.NewManagerId, modifiedBy);

                            #region Manage User Role when role changes
                            //-- Insert User_Activity_Permission data
                            if (oldRoleId != null && oldRoleId != Guid.Empty && newRoleId != null && newRoleId != Guid.Empty && oldRoleId != newRoleId && user.IsDeleted == false)
                            {
                                //-- List of rights for role before role changes (old role).
                                var lstOldRights = db.Role_Activity_Permission.Where(a => a.RoleId == oldRoleId).ToList();

                                //-- List of rights for role that has to be updated (new role).
                                var lstNewRights = db.Role_Activity_Permission.Where(a => a.RoleId == newRoleId).ToList();

                                //-- List of rights of old roles that needs to be deleted as new role assigns.
                                var lstRightsToDelete = lstOldRights.Where(a => !lstNewRights.Select(b => b.ApplicationActivityId).Contains(a.ApplicationActivityId)).ToList();

                                //-- List of rights of new roles that needs to be inserted as new role assigns.
                                var lstRightsToInsert = lstNewRights.Where(a => !lstOldRights.Select(b => b.ApplicationActivityId).Contains(a.ApplicationActivityId)).ToList();

                                //-- Delete old rights
                                if (lstRightsToDelete != null)
                                {
                                    if (lstRightsToDelete.Count > 0)
                                    {
                                        foreach (var item in lstRightsToDelete)
                                        {
                                            var objUser_Activity_Permission = db.User_Activity_Permission.Where(a => a.UserId == user.UserId && a.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault();
                                            if (objUser_Activity_Permission != null)
                                            {
                                                db.Entry(objUser_Activity_Permission).State = EntityState.Deleted;
                                                db.User_Activity_Permission.Remove(objUser_Activity_Permission);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                //-- Insert new rights
                                if (lstRightsToInsert != null)
                                {
                                    if (lstRightsToInsert.Count > 0)
                                    {
                                        foreach (var item in lstRightsToInsert)
                                        {
                                            User_Activity_Permission objUser_Activity_Permission = new User_Activity_Permission();
                                            objUser_Activity_Permission.ApplicationActivityId = item.ApplicationActivityId;
                                            objUser_Activity_Permission.CreatedBy = modifiedBy;
                                            objUser_Activity_Permission.CreatedDate = DateTime.Now;
                                            objUser_Activity_Permission.UserId = user.UserId;
                                            db.Entry(objUser_Activity_Permission).State = EntityState.Added;
                                            db.User_Activity_Permission.Add(objUser_Activity_Permission);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            #endregion
                            // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                        }
                    }
                    scope.Complete();       // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return retVal;
        }

        #endregion

        #region Re-assign manager
        /// <summary>
        /// When a manager profile get deleted, re-assign the subordinates to other manager.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>17/06/2014</CreatedDate>
        /// <param name="UserId"></param>
        /// <param name="NewManagerId"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        public int ReassignManager(Guid UserId, Guid? NewManagerId, Guid modifiedBy)
        {
            try
            {
                var lstSubOrdinated = db.User_Application.Where(a => a.ManagerId == UserId && a.IsDeleted.Equals(false)).ToList();

                if (lstSubOrdinated != null)
                {
                    if (lstSubOrdinated.Count > 0)
                    {
                        foreach (var item in lstSubOrdinated)
                        {
                            item.ManagerId = NewManagerId;
                            item.ModifiedDate = DateTime.Now;
                            item.ModifiedBy = modifiedBy;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }

                return 1;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return -1;
            }
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

        #region Get Application Release Version

        /// <summary>
        /// Get Application Release Version using ApplicationId
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>21/05/2014</CreatedDate>
        /// <Description>To address PL ticket #469.</Description>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public string GetApplicationReleaseVersion(Guid applicationId)
        {
            string appReleaseVersion = string.Empty;
            if (applicationId != null)
            {
                appReleaseVersion = (from a in db.Applications
                                     where a.ApplicationId == applicationId
                                     select (a.ReleaseVersion)).FirstOrDefault();
            }
            return appReleaseVersion;
        }

        #endregion

        #region Get Role List New

        /// <summary>
        /// Function to get role list.
        /// </summary>
        /// added by uday for #513
        /// <returns>Returns roles .</returns>
        public List<BDSEntities.Role> GetAllRoleList(Guid applicationid)
        {
            List<BDSEntities.Role> roleList = new List<BDSEntities.Role>();
            List<Role> rolelist = new List<Role>();

            //rolelist = db.Roles.Where(role => role.IsDeleted == false).ToList();
            rolelist = (from role in db.Roles
                        join approle in db.Application_Role on role.RoleId equals approle.RoleId
                        where approle.ApplicationId == applicationid
                        select role).OrderBy(role => role.Description).ToList();//change review point order by clause
            if (rolelist.Count > 0)
            {
                foreach (var role in rolelist)
                {
                    BDSEntities.Role roleEntity = new BDSEntities.Role();
                    roleEntity.RoleId = role.RoleId;
                    roleEntity.Code = role.Code;
                    roleEntity.Title = role.Title;
                    roleEntity.ColorCode = role.ColorCode;
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

        #region Get Application activity list

        /// <summary>
        /// Function to get Application activity list
        /// </summary>
        /// added by uday for #513
        /// <returns>Application activity list .</returns>
        public List<BDSEntities.ApplicationActivity> GetApplicationactivitylist(Guid applicationid)
        {
            List<BDSEntities.ApplicationActivity> ApplicationActivityList = new List<BDSEntities.ApplicationActivity>();
            List<Application_Activity> ApplicationActivity = new List<Application_Activity>();

            ApplicationActivity = db.Application_Activity.Where(application => application.ApplicationId == applicationid).ToList();
            if (ApplicationActivity.Count > 0)
            {
                foreach (var item in ApplicationActivity)
                {
                    BDSEntities.ApplicationActivity applactlist = new BDSEntities.ApplicationActivity();
                    applactlist.ApplicationActivityId = item.ApplicationActivityId;
                    applactlist.ApplicationId = item.ApplicationId;
                    applactlist.CreatedDate = item.CreatedDate;
                    applactlist.ActivityTitle = item.ActivityTitle;
                    applactlist.ParentId = Convert.ToInt32(item.ParentId);
                    ApplicationActivityList.Add(applactlist);
                }
            }

            return ApplicationActivityList;
        }

        #endregion

        #region DuplicateRoleCheck

        /// <summary>
        /// Function to insert new Role
        /// </summary>//added by uday #513
        /// <param name="user">user entity</param>
        /// <param name="applicationId">application</param>
        /// <param name="createdBy">created by this user</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int DuplicateRoleCheck(BDSEntities.Role role, Guid applicationid)
        {
            int retVal = 0;
            try
            {
                var objDuplicateCheck = (from roles in db.Roles
                                         join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                                         where approle.ApplicationId == applicationid && roles.Description == role.Description
                                         select roles.Description).FirstOrDefault();
                if (objDuplicateCheck != null)
                {
                    retVal = -1;
                }
                else
                {
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

        #region Get List Of Users for particular role

        /// <summary>
        /// Function to get Get List Of Users for particular role #513
        /// </summary>
        /// <param name="clientId">client</param>
        /// <param name="applicationId">application</param>
        /// <param name="userId">user</param>
        /// <param name="isSystemAdmin">whether user is admin of or not</param>
        /// <returns>Returns Get List Of Users for particular role.</returns>
        public List<BDSEntities.User> GetRoleMemberList(Guid applicationId, Guid roleid)
        {
            List<BDSEntities.User> roleMemberList = new List<BDSEntities.User>();
            List<User> lstUser = (from user in db.Users
                                  join ua in db.User_Application on user.UserId equals ua.UserId
                                  where ua.RoleId == roleid && ua.ApplicationId == applicationId && user.IsDeleted == false
                                  select user).OrderBy(q => q.FirstName).ToList();
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
                    roleMemberList.Add(userEntity);
                }
            }
            return roleMemberList;
        }

        #endregion

        #region  Add new role and its permissions

        /// <summary>
        /// Function Add new role and its permissions #51
        /// </summary>
        /// <param name="role id">role id</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int CreateRole(string roledesc, string permissionID, string colorcode, Guid applicationid, Guid createdby, Guid roleid)
        {
            int retVal = 0;
            Guid NewRoleId = Guid.NewGuid();
            try
            {
                //Insert in Role
                var obj = (from roles in db.Roles
                           join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                           where approle.ApplicationId == applicationid && roles.RoleId == roleid
                           select roles).FirstOrDefault();

                var objnew = (from roles in db.Roles
                              join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                              where approle.ApplicationId == applicationid && roles.RoleId == roleid
                              select approle).FirstOrDefault();

                if (obj != null)
                {
                    obj.ColorCode = colorcode;
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Role objrole = new Role();
                    objrole.RoleId = NewRoleId;
                    objrole.Code = "";
                    objrole.Title = roledesc;
                    //objrole.Description = roledesc;
                    objrole.CreatedBy = createdby;
                    objrole.CreatedDate = DateTime.Now;
                    objrole.IsDeleted = false;
                    objrole.ColorCode = colorcode;

                    db.Entry(objrole).State = EntityState.Added;
                    db.Roles.Add(objrole);
                    db.SaveChanges();
                }

                //Insert in Application_role
                if (objnew != null)
                {
                }
                else
                {
                    Application_Role objApplication_Role = new Application_Role();
                    objApplication_Role.ApplicationId = applicationid;
                    objApplication_Role.RoleId = NewRoleId;
                    objApplication_Role.CreatedBy = createdby;
                    objApplication_Role.CreatedDate = DateTime.Now;
                    objApplication_Role.IsDeleted = false;
                    db.Entry(objApplication_Role).State = EntityState.Added;
                    db.Application_Role.Add(objApplication_Role);
                    db.SaveChanges();
                }

                ////Insert in [Role_Activity_Permission]
                if (roleid != Guid.Empty)
                {
                    var objdelete = db.Role_Activity_Permission.Where(roleapp => roleapp.RoleId == roleid).ToList();
                    if (objdelete.Count > 0)
                    {
                        foreach (var item in objdelete)
                        {
                            db.Entry(item).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                    }
                    string[] id = permissionID.Split(',');
                    if (id.Length > 0)
                    {
                        for (int i = 0; i < id.Length; i++)
                        {
                            string[] strarr = id[i].Split('_');
                            if (strarr.Contains("true"))
                            {
                                Role_Activity_Permission objactivitypermission = new Role_Activity_Permission();
                                objactivitypermission.ApplicationActivityId = Convert.ToInt32(strarr[1]);
                                objactivitypermission.RoleId = roleid;
                                objactivitypermission.CreatedBy = createdby;
                                objactivitypermission.CreatedDate = DateTime.Now;
                                db.Entry(objactivitypermission).State = EntityState.Added;
                                db.Role_Activity_Permission.Add(objactivitypermission);
                                db.SaveChanges();

                            }
                        }
                    }
                }
                else
                {
                    string[] id = permissionID.Split(',');
                    if (id.Length > 0)
                    {
                        for (int i = 0; i < id.Length; i++)
                        {
                            string[] strarr = id[i].Split('_');
                            if (strarr.Contains("true"))
                            {
                                Role_Activity_Permission objactivitypermission = new Role_Activity_Permission();
                                objactivitypermission.ApplicationActivityId = Convert.ToInt32(strarr[1]);
                                objactivitypermission.RoleId = NewRoleId;
                                objactivitypermission.CreatedBy = createdby;
                                objactivitypermission.CreatedDate = DateTime.Now;
                                db.Entry(objactivitypermission).State = EntityState.Added;
                                db.Role_Activity_Permission.Add(objactivitypermission);
                                db.SaveChanges();

                            }
                        }
                    }
                }
                retVal = 1;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retVal;
        }

        #endregion

        #region delete role and reassign new role to existing users

        /// <summary>
        /// 3delete role and reassign new role to existing users. #513
        /// </summary>
        /// <param name="role id">role id</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int DeleteRoleAndReassign(Guid delroleid, Guid? reassignroleid, Guid applicationid)
        {
            int retVal = 0;
            try
            {
                var obj = (from roles in db.Roles
                           join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                           where approle.ApplicationId == applicationid && roles.RoleId == delroleid
                           select roles).FirstOrDefault();

                var objnew = (from roles in db.Roles
                              join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                              where approle.ApplicationId == applicationid && roles.RoleId == delroleid
                              select approle).FirstOrDefault();

                var objuser = db.User_Application.Where(userapp => userapp.ApplicationId == applicationid && userapp.RoleId == delroleid).ToList();

                var objrolepermisson = (from roles in db.Role_Permission
                                        join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                                        where approle.ApplicationId == applicationid && roles.RoleId == delroleid
                                        select roles).ToList();

                var objappactivity = db.Role_Activity_Permission.Where(roleapp => roleapp.RoleId == delroleid).ToList();

                if (obj == null || objnew == null || objuser == null)
                {
                    retVal = -1;
                }
                else
                {
                    db.Entry(objnew).State = EntityState.Deleted;
                    db.SaveChanges();
                    if (objuser.Count > 0 && reassignroleid != null && reassignroleid != Guid.Empty)
                    {
                        foreach (var item in objuser)
                        {
                            item.RoleId = reassignroleid.Value;

                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    foreach (var item in objrolepermisson)
                    {
                        db.Entry(item).State = EntityState.Deleted;
                        db.SaveChanges();
                    }

                    foreach (var item in objappactivity)
                    {
                        db.Entry(item).State = EntityState.Deleted;
                        db.SaveChanges();
                    }

                    db.Entry(obj).State = EntityState.Deleted;
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

        #region  Copy role and its permissions

        /// <summary>
        /// Function Copy role and its permissions. #513
        /// </summary>
        /// <param name="role id">role id</param>
        /// <returns>Returns 1 if the operation is successful, 0 otherwise.</returns>
        public int CopyRole(string copyroledesc, Guid originalid, Guid applicationid, Guid createdby)
        {
            int retVal = 0;
            try
            {
                //Insert in Role

                var objrole = (from roles in db.Roles
                               join approle in db.Application_Role on roles.RoleId equals approle.RoleId
                               where approle.ApplicationId == applicationid && roles.RoleId == originalid
                               select roles).FirstOrDefault();
                Role objrolenew;

                Guid NewRoleId = Guid.Empty;
                if (objrole != null)
                {
                    objrolenew = new Role();

                    NewRoleId = Guid.NewGuid();
                    objrolenew.RoleId = NewRoleId;
                    objrolenew.Code = "";
                    objrolenew.Title = copyroledesc;
                    //objrolenew.Description = copyroledesc;
                    objrolenew.CreatedBy = createdby;
                    objrolenew.CreatedDate = DateTime.Now;
                    objrolenew.IsDeleted = false;
                    objrolenew.ColorCode = objrole.ColorCode;

                    db.Entry(objrolenew).State = EntityState.Added;
                    db.Roles.Add(objrolenew);
                    db.SaveChanges();
                }
                //Insert in Application_role
                Application_Role objApplication_Role = new Application_Role();
                objApplication_Role.ApplicationId = applicationid;
                objApplication_Role.RoleId = NewRoleId;
                objApplication_Role.CreatedBy = createdby;
                objApplication_Role.CreatedDate = DateTime.Now;
                objApplication_Role.IsDeleted = false;
                db.Entry(objApplication_Role).State = EntityState.Added;
                db.Application_Role.Add(objApplication_Role);
                db.SaveChanges();


                ////Insert in [Role_Activity_Permission]
                var objappactivity = db.Role_Activity_Permission.Where(roleapp => roleapp.RoleId == originalid).ToList();
                if (objappactivity.Count > 0)
                {
                    foreach (var item in objappactivity)
                    {
                        Role_Activity_Permission objactivitypermission = new Role_Activity_Permission();
                        objactivitypermission.ApplicationActivityId = item.ApplicationActivityId;
                        objactivitypermission.RoleId = NewRoleId;
                        objactivitypermission.CreatedBy = createdby;
                        objactivitypermission.CreatedDate = DateTime.Now;
                        db.Entry(objactivitypermission).State = EntityState.Added;
                        db.Role_Activity_Permission.Add(objactivitypermission);
                        db.SaveChanges();
                    }
                }
                retVal = 1;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retVal;
        }

        #endregion

        #region Get List Of roleactivitypermissions

        /// <summary>
        /// Function to get Get List Of Users for particular role #513
        /// </summary>
        /// <param name="clientId">client</param>
        /// <param name="applicationId">application</param>
        /// <param name="userId">user</param>
        /// <param name="isSystemAdmin">whether user is admin of or not</param>
        /// <returns>Returns Get List roleactivitypermissions.</returns>
        public List<BDSEntities.ApplicationActivity> GetRoleactivitypermissions(Guid roleid)
        {
            List<BDSEntities.ApplicationActivity> roleactivitypermissions = new List<BDSEntities.ApplicationActivity>();

            List<Application_Activity> ApplicationActivity = db.Role_Activity_Permission.Where(roleactivity => roleactivity.RoleId == roleid).Select(roleActivity => roleActivity.Application_Activity).ToList();


            if (ApplicationActivity.Count > 0)
            {
                foreach (var item in ApplicationActivity)
                {
                    BDSEntities.ApplicationActivity applactlist = new BDSEntities.ApplicationActivity();
                    applactlist.ApplicationActivityId = item.ApplicationActivityId;
                    applactlist.ApplicationId = item.ApplicationId;
                    applactlist.CreatedDate = item.CreatedDate;
                    applactlist.ActivityTitle = item.ActivityTitle;
                    applactlist.ParentId = Convert.ToInt32(item.ParentId);
                    roleactivitypermissions.Add(applactlist);
                }
            }

            return roleactivitypermissions;
        }

        #endregion

        #region "Application Activity"

        public List<BDSEntities.ApplicationActivity> GetAllApplicationActivity(Guid applicationId)
        {

            var appActivity = db.Application_Activity.Where(act => act.ApplicationId == applicationId).Select(act => new BDSEntities.ApplicationActivity
            {
                ApplicationActivityId = act.ApplicationActivityId,
                ApplicationId = act.ApplicationId,
                ParentId = act.ParentId,
                CreatedDate = act.CreatedDate,
                ActivityTitle = act.ActivityTitle

            }).ToList();
            if (appActivity.Count > 0)
            {
                return appActivity;
            }
            return null;
        }

        public List<BDSEntities.UserApplicationPermission> GetUserActivity(Guid userId, Guid applicationId)
        {
            var userActivity = db.User_Activity_Permission.Where(usr => usr.UserId == userId).Select(usr => new BDSEntities.UserApplicationPermission
            {
                UserId = usr.UserId,
                ApplicationActivityId = usr.ApplicationActivityId,
                CreatedDate = usr.CreatedDate,
                CreatedBy = usr.CreatedBy

            }).ToList();
            if (userActivity.Count > 0)
            {
                return userActivity;
            }
            return null;

        }

        public List<BDSEntities.CustomRestriction> GetUserCustomRestrictionList(Guid userId, Guid applicationId)
        {
            var usercustomRestrictionList = db.CustomRestrictions.Where(crl => crl.UserId == userId).Select(crl => new BDSEntities.CustomRestriction
            {
                UserId = crl.UserId,
                CustomField = crl.CustomField,
                CustomFieldId = crl.CustomFieldId,
                Permission = crl.Permission,
                CreatedDate = crl.CreatedDate,
                CreatedBy = crl.CreatedBy
            }).ToList();
            if (usercustomRestrictionList.Count > 0)
            {
                return usercustomRestrictionList;
            }
            return null;

        }
        public int DeleteUserActivityPermission(Guid userId, Guid applicationId)
        {
            int retVal = 0;
            var userActivityList = db.User_Activity_Permission.Where(usr => usr.UserId == userId).ToList();
            if (userActivityList.Count > 0)
            {
                foreach (var activity in userActivityList)
                {
                    db.Entry(activity).State = EntityState.Deleted;
                    db.User_Activity_Permission.Remove(activity);
                    db.SaveChanges();
                    retVal = 1;
                }
                return retVal;
            }
            return retVal;
        }

        public int DeleteUserCustomrestriction(Guid userId, Guid applicationId)
        {
            int retVal = 0;
            var userCustomrestrictionList = db.CustomRestrictions.Where(usr => usr.UserId == userId).ToList();
            if (userCustomrestrictionList.Count > 0)
            {
                foreach (var customRestriction in userCustomrestrictionList)
                {
                    db.Entry(customRestriction).State = EntityState.Deleted;
                    db.CustomRestrictions.Remove(customRestriction);
                    db.SaveChanges();
                    retVal = 1;
                }
                return retVal;
            }
            return retVal;
        }

        public int AddUserActivityPermissions(Guid userId, Guid CreatorId, string[] permissions, Guid applicationId)
        {
            int retVal = 0;
            if (permissions.Length > 0)
            {
                DeleteUserActivityPermission(userId, applicationId);
                DeleteUserCustomrestriction(userId, applicationId);
                foreach (var item in permissions)
                {

                    if (item.ToLower().Contains("yes"))
                    {
                        string[] splitpermissions = item.Split('_');
                        User_Activity_Permission obj = new User_Activity_Permission();
                        obj.UserId = userId;
                        obj.ApplicationActivityId = Convert.ToInt32(splitpermissions[2]);
                        obj.CreatedBy = CreatorId;
                        obj.CreatedDate = System.DateTime.Now;
                        db.Entry(obj).State = EntityState.Added;
                        db.User_Activity_Permission.Add(obj);
                        db.SaveChanges();
                        retVal = 1;
                    }
                    else if (item.ToLower().Contains("verticals"))
                    {
                        string[] splitpermissions = item.Split('_');
                        CustomRestriction obj = new CustomRestriction();
                        obj.UserId = userId;
                        obj.CustomFieldId = splitpermissions[2];
                        obj.CustomField = "Verticals";
                        obj.Permission = Convert.ToInt16(splitpermissions[0]);
                        obj.CreatedDate = System.DateTime.Now;
                        obj.CreatedBy = CreatorId;
                        db.Entry(obj).State = EntityState.Added;
                        db.CustomRestrictions.Add(obj);
                        db.SaveChanges();
                        retVal = 1;
                    }
                    else if (item.ToLower().Contains("geography"))
                    {
                        string[] splitpermissions = item.Split('_');
                        CustomRestriction obj = new CustomRestriction();
                        obj.UserId = userId;
                        obj.CustomFieldId = splitpermissions[2];
                        obj.CustomField = "Geography";
                        obj.Permission = Convert.ToInt16(splitpermissions[0]);
                        obj.CreatedDate = System.DateTime.Now;
                        obj.CreatedBy = CreatorId;
                        db.Entry(obj).State = EntityState.Added;
                        db.CustomRestrictions.Add(obj);
                        db.SaveChanges();
                        retVal = 1;
                    }
                }

            }
            return retVal;
        }

        public int resetToRoleDefault(Guid userId, Guid CreatorId, Guid applicationId)
        {
            int retVal = 0;
            var roleId = db.User_Application.Where(usr => usr.UserId == userId && usr.ApplicationId == applicationId).FirstOrDefault().RoleId;
            var DefaultActivities = db.Role_Activity_Permission.Where(rap => rap.RoleId == roleId).ToList();
            if (DefaultActivities.Count > 0)
            {
                DeleteUserActivityPermission(userId, applicationId);
                foreach (var item in DefaultActivities)
                {

                    User_Activity_Permission obj = new User_Activity_Permission();
                    obj.UserId = userId;
                    obj.ApplicationActivityId = item.ApplicationActivityId;
                    obj.CreatedBy = CreatorId;
                    obj.CreatedDate = System.DateTime.Now;
                    db.Entry(obj).State = EntityState.Added;
                    db.User_Activity_Permission.Add(obj);
                    db.SaveChanges();
                    retVal = 1;
                }
            }
            return retVal;
        }
        #endregion

        #region User hierarchy

        /// <summary>
        /// Get All users and respective managers by applicationId and clientId
        /// Dharmraj, Ticket #520
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public List<BDSEntities.UserHierarchy> GetUserHierarchy(Guid clientId, Guid applicationId)
        {
            List<BDSEntities.UserHierarchy> lstUserHierarchy = (
                            from u in db.Users
                            join ua in db.User_Application on u.UserId equals ua.UserId
                            join r in db.Roles on ua.RoleId equals r.RoleId
                            where u.IsDeleted == false && u.ClientId == clientId && ua.ApplicationId == applicationId
                            select new BDSEntities.UserHierarchy
                            {
                                UserId = u.UserId,
                                Email = u.Email,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                RoleId = r.RoleId,
                                RoleTitle = r.Title,
                                ColorCode = r.ColorCode,
                                JobTitle = u.JobTitle,
                                GeographyId = u.GeographyId,
                                Phone = u.Phone,
                                ManagerId = ua.ManagerId
                            }
                          ).ToList();

            return lstUserHierarchy;

        }

        #endregion
    }
}
