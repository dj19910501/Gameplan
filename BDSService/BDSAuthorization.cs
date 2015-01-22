using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDSService.Helpers;

namespace BDSService
{
    public class BDSAuthorization
    {
        #region Variables

        private BDSAuthEntities db = new BDSAuthEntities();

        #endregion

        #region Validate User

        public BDSEntities.User ValidateUser(Guid applicationId, string userEmail, string userPassword)
        {
            BDSEntities.User userObj = new BDSEntities.User();

            byte[] saltbytes = Common.GetSaltBytes();

            string HashPassword = Common.ComputeFinalHash(userPassword, saltbytes);
            //Start PL#861 New User's Login Issues Manoj 22Oct2014
            //var user = (from u in db.Users
            //            join ua in db.User_Application on u.UserId equals ua.UserId
            //            where u.Email == userEmail && u.Password == HashPassword && ua.ApplicationId == applicationId && u.IsDeleted == false
            //            select new { u }).SingleOrDefault();
            var user = (from u in db.Users
                        join ua in db.User_Application on u.UserId equals ua.UserId
                        where u.Email == userEmail && u.Password == HashPassword && ua.ApplicationId == applicationId && u.IsDeleted == false && ua.IsDeleted == false
                        select new { u }).FirstOrDefault();
            //End PL#861 New User's Login Issues Manoj 22Oct2014
            if (user == null)
            {
                userObj = null;
            }
            else
            {
                userObj.UserId = user.u.UserId;
                userObj.ClientId = user.u.ClientId;
                userObj.Client = db.Clients.Where(cl => cl.ClientId == user.u.ClientId).Select(c => c.Name).FirstOrDefault();
                userObj.DisplayName = user.u.DisplayName;
                userObj.Email = user.u.Email;
                userObj.FirstName = user.u.FirstName;
                userObj.JobTitle = user.u.JobTitle;
                userObj.LastName = user.u.LastName;
                userObj.ProfilePhoto = user.u.ProfilePhoto;
                userObj.RoleId = db.User_Application.Where(ua => ua.ApplicationId == applicationId && ua.UserId == user.u.UserId).Select(u => u.RoleId).FirstOrDefault();
                userObj.RoleCode = db.Roles.Where(rl => rl.RoleId == userObj.RoleId).Select(r => r.Code).FirstOrDefault();
                userObj.LastLoginDate = user.u.User_Application.Select(l => l.LastLoginDate).FirstOrDefault();
                userObj.SecurityQuestionId = user.u.SecurityQuestionId;
                userObj.Answer = user.u.Answer;
            }
            return userObj;
        }

        #endregion
    }
}
