using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RevenuePlanner.Models;
using RevenuePlanner.BDSService;
using Elmah;
using RevenuePlanner.Helpers;
using System.Data;
using System.Transactions;
using RevenuePlanner.BAL;

namespace RevenuePlanner.Controllers
{
    public class OrganizationController : CommonController
    {
        //
        // GET: /Organization/

        #region "Variables"
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        private MRPEntities db = new MRPEntities();
        #endregion

        #region "Index"
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region "Roles related Methods"
        /// <summary>
        /// Manage Role.
        /// </summary>
        /// <returns>Return ManageRoles view with RoleModel.</returns>
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult ManageRoles()
        {
            // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            List<RoleModel> listrolemodel = new List<RoleModel>();
            try
            {
                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();

                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                var memberlist = bdsuserrepository.GetAllRoleListEx(Sessions.ApplicationId, Sessions.User.CID);

                // Added by dharmraj to remove above foreach loop, 10-7-2014
                //// Get Members Role details.
                listrolemodel = memberlist.Select(member => new RoleModel
                                                 {
                                                     RoleTitle = member.Title,
                                                     RoleCode = member.Code,
                                                     RoleId = member.RoleId
                                                 }).ToList();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }

            return View(listrolemodel.AsEnumerable());
        }

        /// <summary>
        /// Add new Role.
        /// </summary>
        /// <param name="roledesc">Role Decription</param>
        /// <returns>Return flag value in Json</returns>
        [HttpPost]
        public JsonResult AddRole(string roledesc)
        {
            if (string.IsNullOrEmpty(roledesc))
                return Json(false);

            //// Add New Role description to Tempdata.
            BDSService.Role objrole = new BDSService.Role();
            objrole.Description = roledesc;////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            objrole.Title = roledesc;////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            TempData["objrole"] = objrole;

            try
            {
                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 

                //// Check Role duplication.
                int retval = objBDSServiceClient.DuplicateRoleCheckEx(objrole, Sessions.ApplicationId, Sessions.User.CID);

                //// if duplicate role then return false otherwise true.
                if (retval == 1)
                    return Json(true);
                else
                    return Json(false);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false);
        }

        /// <summary>
        /// Edit Role.
        /// <param name="roleId">Edit Role details by RoleId</param>
        /// </summary>
        /// <returns>Return RoleEdit view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult RoleEdit(Guid roleId)//changed by uday for functional review point...3-7-2014;
        {
            // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            try
            {
                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();

                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                var rolelist = bdsuserrepository.GetAllRoleListEx(Sessions.ApplicationId, Sessions.User.CID);
                if (roleId == Guid.Empty)//changed by uday for functional review point...3-7-2014;
                {
                    var prevrole = (Role)TempData["objrole"];//changed by uday for functional review point...3-7-2014;
                    if (prevrole != null)
                    {
                        ViewBag.current = prevrole.Description;
                    }
                    ViewBag.roleid = roleId;
                }
                else
                {
                    ViewBag.current = rolelist.Where(role => role.RoleId == roleId).Select(role => role.Title).FirstOrDefault();
                    ViewBag.roleid = roleId;
                    ViewBag.colorcode = rolelist.Where(role => role.RoleId == roleId).Select(role => role.ColorCode).FirstOrDefault();

                    rolelist = rolelist.Where(role => role.RoleId != roleId).ToList();
                }
                List<RoleModel> listrolemodel = new List<RoleModel>();
                IList<SelectListItem> RoleList = new List<SelectListItem>();
                RoleList = rolelist.Select(role => new SelectListItem() { Text = role.Title, Value = role.RoleId.ToString(), Selected = false })
                                .OrderBy(it => it.Text, new AlphaNumericComparer()).ToList();

                //// Get Application Activity.
                var activitylist = bdsuserrepository.GetUserApplicationactivitylist(Sessions.ApplicationId);

                //// Get Role Activity permissions by RoleId.
                var filterlist = bdsuserrepository.GetRoleactivitypermissions(roleId);
                ViewData["permissionlist"] = filterlist;
                ViewData["activitylist"] = activitylist;

                string ids = string.Empty;
                if (filterlist.Count > 0)
                {
                    ids = string.Join(",", filterlist.Select(filter => filter.ApplicationActivityId.ToString()).ToArray());
                }//commented by uday for functional review point...3-7-2014
                ViewData["permissionids"] = ids.ToString();
                TempData["RoleList"] = new SelectList(RoleList, "Value", "Text");
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }

            return View();
        }

        /// <summary>
        /// Delete Role.
        /// <param name="roleid">User RoleId</param>
        /// <param name="selectedrole">Role Text</param>
        /// </summary>
        /// <returns>Return RoleDelete partialview.</returns>
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult RoleDelete(Guid roleid, string selectedrole)
        {
            try
            {
                ViewData["users"] = objBDSServiceClient.GetRoleMemberList(Sessions.ApplicationId, roleid);
                //changed by uday for functional review point...3-7-2014;

                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();

                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                var rolelistData = bdsuserrepository.GetAllRoleListEx(Sessions.ApplicationId, Sessions.User.CID).Where(rolelist => rolelist.RoleId != roleid).ToList().Select(role => new SelectListItem()
                {
                    Text = role.Title,
                    Value = Convert.ToString(role.RoleId),
                }).OrderBy(role => role.Text, new AlphaNumericComparer());

                ViewData["roles"] = rolelistData;
                ViewData["roleselected"] = selectedrole;
                ViewData["deleterole"] = roleid;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            return PartialView("RoleDelete");//name change by uday for functional review point...3-7-2014
        }

        /// <summary>
        /// Check User Role Permission.
        /// <param name="roleid">User RoleId</param>
        /// <param name="permission">Role Text</param>
        /// </summary>
        /// <returns>Return CheckPermission partialview.</returns>
        public ActionResult CheckPermission(Guid roleid, string permission)
        {
            try
            {
                List<User> user_list = new List<User>();

                //// Get User Role mapping list by ApplicationId and RoleId.
                var user_role_mapping = objBDSServiceClient.GetRoleMemberList(Sessions.ApplicationId, roleid);

                //// Get Application Activity List.
                var activitylist = objBDSServiceClient.GetUserApplicationactivitylist(Sessions.ApplicationId);
                List<int> idsList = new List<int>();
                foreach (string id in permission.Split(','))
                {
                    idsList.Add(Convert.ToInt32(id));
                }

                //// filter Activity list.
                var activity_CodeList = activitylist.Where(activity => idsList.Contains(activity.ApplicationActivityId)).ToList();

                //// Get list of Users using User_Role mapping list.
                List<string> user_activity_mapping;
                foreach (var user_role_map in user_role_mapping)
                {
                    user_activity_mapping = new List<string>();
                    user_activity_mapping = objBDSServiceClient.GetUserActivityPermission(user_role_map.UserId, Sessions.ApplicationId);
                    if (activity_CodeList.Where(activity => user_activity_mapping.Contains(activity.Code)).ToList().Count() > 0)
                    {
                        user_list.Add(user_role_map);
                    }
                }

                ViewData["users"] = user_list;
                ViewData["apptitle"] = activity_CodeList.Select(listt => listt.ActivityTitle).ToList();
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            return PartialView("CheckPermission");
        }

        /// <summary>
        /// Delete Role by RoelId.
        /// <param name="delroleid">Delete RoleId</param>
        /// <param name="reassignroleid">Reassign new RoleId</param>
        /// <param name="LoginId">Check loginId with current loggined UserId</param>
        /// </summary>
        /// <returns>Return status flag value. in jsonresult</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteRole(Guid delroleid, Guid reassignroleid, Guid LoginId = new Guid()) // change type of parameter userid from interger to guid #2953 
        {
            // Start - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
           
                if (!Sessions.User.UserId.Equals(LoginId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            
            // End - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

            if (reassignroleid == null)
                reassignroleid = Guid.Empty;

            try
            {
                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 

                //// Delete role and reassign new role value.
                int retval = objBDSServiceClient.DeleteRoleAndReassignEx(delroleid, reassignroleid, Sessions.ApplicationId, Sessions.User.ID, Sessions.User.CID);

                if (retval == 1)
                {
                    TempData["SuccessMessage"] = Common.objCached.RoleDeleteSuccess;
                    return Json(new { status = true }, JsonRequestBehavior.AllowGet);  // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);  // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create New Role.
        /// <param name="roledesc">Role Description</param>
        /// <param name="checkbox">CheckboxId</param>
        /// <param name="colorcode">Color code</param>
        /// <param name="roleid">RoleId</param>
        /// <param name="delpermission"></param>
        /// <param name="LoginId">LoginId</param>
        /// </summary>
        /// <returns>Return status flag value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(string roledesc, string checkbox, string colorcode, Guid roleid, string delpermission, Guid LoginId = new Guid())// change type of parameter LoginId from interger to guid #2953 
        {
            try
            {
                // Start - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                
                    if (Sessions.User.UserId != LoginId)
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                
                // End - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

                string permissionID = checkbox.ToString();

                //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 

                //// Create new Role with Details.
                int retval = objBDSServiceClient.CreateRoleEx(roledesc, permissionID, colorcode, Sessions.ApplicationId, Sessions.User.ID, roleid, delpermission, Sessions.User.CID);
                if (retval == 1)
                {
                    return Json(new { status = true }, JsonRequestBehavior.AllowGet);  // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);  // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create clone of Role.
        /// <param name="copyroledesc">Copy Role Description</param>
        /// <param name="originalroleid">Original RoleID</param>
        /// <param name="LoginId">User LoginId</param>
        /// </summary>
        /// <returns>Return status flag value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CopyRole(string copyroledesc, Guid originalroleid, Guid LoginId = new Guid())// change type of parameter LoginId from interger to guid #2953 
        {
            // Start - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

            if (Sessions.User.UserId != LoginId)
            {
                TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
            }
            
            // End - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

            if (!string.IsNullOrEmpty(copyroledesc))
            {
                BDSService.Role objrole = new BDSService.Role();
                objrole.Description = copyroledesc;
                objrole.Title = copyroledesc.Trim();
                Session["session"] = objrole;
                try
                {
                    //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                    int retvalcheck = objBDSServiceClient.DuplicateRoleCheckEx(objrole, Sessions.ApplicationId, Sessions.User.CID);

                    //// Not Duplicate Role.
                    if (retvalcheck == 1)
                    {
                        //// Copy Role.
                        int retval = objBDSServiceClient.CopyRoleEx(copyroledesc.Trim(), originalroleid, Sessions.ApplicationId, Sessions.User.ID, Sessions.User.CID);
                        if (retval == 1)
                        {
                            TempData["SuccessMessage"] = Common.objCached.RoleCopySuccess;
                            return Json(new { status = true }, JsonRequestBehavior.AllowGet);   // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                        }
                        else
                        {
                            return Json(new { status = false }, JsonRequestBehavior.AllowGet);   // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                        }
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);   // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    if (e is System.ServiceModel.EndpointNotFoundException)
                    {
                        //// Flag to indicate unavailability of web service.
                        //// Added By: Maninder Singh Wadhva on 11/24/2014.
                        //// Ticket: 942 Exception handeling in Gameplan.
                        return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);   // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
        }
        #endregion

        #region "User Hierarchy related Methods"
        /// <summary>
        /// Organization Hierarchy
        /// </summary>
        /// <returns>Return OrganizationHierarchy view</returns>
        public ActionResult OrganizationHierarchy()
        {
            // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            try
            {
                List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
                lstUserHierarchy = objBDSService.GetUserHierarchyEx(Sessions.User.CID, Sessions.ApplicationId);
                // To sort all users by role title, by Dharmraj, #579
                lstUserHierarchy = lstUserHierarchy.OrderBy(u => u.RoleTitle).ToList();

                //// Get list of Manager Users.
                List<BDSService.UserHierarchy> lstManagerUserHierarchy = lstUserHierarchy.Where(user => user.ManagerId == null).ToList();

                var result = lstManagerUserHierarchy.Select(u => CreateUserHierarchy(lstUserHierarchy, u)).ToList();
                ViewBag.UserHierarchy = result;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View();
        }

        /// <summary>
        /// Function to get all sub users of manager user
        /// Dharmraj, Ticket #520
        /// </summary>
        /// <param name="lstUserHierarchy"></param>
        /// <param name="managerId"></param>
        /// <returns>Return list of UserHierarchy</returns>
        public List<BDSService.UserHierarchy> GetSubUsers(List<BDSService.UserHierarchy> lstUserHierarchy, int managerId)
        {
            if (managerId == 0)
                return null;
            else
                return lstUserHierarchy.Where(user => user.MID == managerId).ToList();
        }

        /// <summary>
        /// Function for making user hierarchy
        /// Dharmraj, Ticket #520
        /// </summary>
        /// <param name="lstUserHierarchy"></param>
        /// <param name="objUserHierarchy"></param>
        /// <returns>Returns UserHierarchyModel</returns>
        public UserHierarchyModel CreateUserHierarchy(List<BDSService.UserHierarchy> lstUserHierarchy, BDSService.UserHierarchy objUserHierarchy)
        {
            //// Set User Hierarchy details.
            int userid = objUserHierarchy.UID;
            string FirstName = objUserHierarchy.FirstName;
            string LastName = objUserHierarchy.LastName;
            string Email = objUserHierarchy.Email;
            Guid RoleId = objUserHierarchy.RoleId;
            string RoleTitle = objUserHierarchy.RoleTitle;
            string ColorCode = objUserHierarchy.ColorCode;
            string JobTitle = objUserHierarchy.JobTitle;
            string Phone = objUserHierarchy.Phone;
            Guid? ManagerId = objUserHierarchy.ManagerId;
            Guid UserGuid = objUserHierarchy.UserId;
            var subUsers = GetSubUsers(lstUserHierarchy, userid)
              .Select(r => CreateUserHierarchy(lstUserHierarchy, r))
              .ToList();
            // Modified by :- Sohel Pathan on 18/17/2014 for PL ticket #594.
            return new UserHierarchyModel { UserId = userid, FirstName = FirstName, LastName = LastName, Email = Email, RoleId = RoleId, RoleTitle = RoleTitle, ColorCode = ColorCode, JobTitle = JobTitle, Phone = Phone, ManagerId = ManagerId,UserGuid=UserGuid, subUsers = subUsers };
        }
        #endregion

        #region "Permission related Methods"
        /// <summary>
        /// Added By: Mitesh Vaishnav for PL ticket #521
        /// View/Edit user permission.
        /// <param name="Id">UserId</param>
        /// <param name="Mode">UserPermission mode</param>
        /// </summary>
        public ActionResult ViewEditPermission(Guid UserGuid, string Mode)// change type of parameter UserId from interger to guid #2953 
        {
            List<UserActivityPermissionModel> userActivityPermissionList = new List<UserActivityPermissionModel>();
            int Id = 0;
            try
            {
                //// Start - Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
                ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

                //// Check User permissions.
                if ((bool)ViewBag.IsUserAdminAuthorized == false && Mode.ToLower() != Enums.UserPermissionMode.View.ToString().ToLower() && Mode.ToLower() != Enums.UserPermissionMode.MyPermission.ToString().ToLower())
                    return RedirectToAction("Index", "NoAccess");

                //called method to get interger userid from GUID
                Id = Common.GetIntegerUserId(UserGuid);
                
                //// End - Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
                BDSService.User objUser = objBDSServiceClient.GetTeamMemberDetailsEx(Id, Sessions.ApplicationId); //This is cross client check, #2878 Security - Account Creation – Client Id and User Id
                if (objUser.CID != Sessions.User.CID) //here we check the cross client data edit
                {
                    TempData["ErrorMessage"] = Common.objCached.UserPermissionRestrictionMessage;
                    return RedirectToAction("OrganizationHierarchy", "Organization");
                }
                ViewBag.PermissionMode = Mode;
                int UserId = Id;
                if (UserId != 0)
                {
                    ViewBag.userId = UserGuid;

                    //// Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                    var roleColorCode = objBDSServiceClient.GetAllRoleListEx(Sessions.ApplicationId, Sessions.User.CID).Where(rol => rol.RoleId == objUser.RoleId).FirstOrDefault().ColorCode;

                    ViewBag.RoleColorCode = roleColorCode;
                    ViewBag.Name = objUser.FirstName + " " + objUser.LastName;
                    ViewBag.RoleName = objUser.RoleTitle;

                    var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);   //// Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139
                    var allActivity = objBDSServiceClient.GetUserApplicationactivitylist(Sessions.ApplicationId);
                    var userActivity = objBDSServiceClient.GetUserActivityEx(UserId, Sessions.ApplicationId);

                    UserActivityPermissionModel uapobj;
                    foreach (var item in allActivity)
                    {
                        //// Set data to User_Activity_Permission model.
                        uapobj = new UserActivityPermissionModel();
                        uapobj.ApplicationActivityId = item.ApplicationActivityId;
                        uapobj.ApplicationId = item.ApplicationId;
                        uapobj.CreatedDate = item.CreatedDate;
                        uapobj.ParentId = item.ParentId;
                        uapobj.Title = item.ActivityTitle;
                        uapobj.ItemCode = item.Code;    //Added by Rahul Shah on 06/10/2015 for PL#1638
                        uapobj.Permission = Enums.UserActivityPermissionType.No.ToString();
                        if (userActivity != null)
                        {
                            if (userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).ToList().Count > 0)
                            {
                                uapobj.Permission = Enums.UserActivityPermissionType.Yes.ToString();
                                uapobj.UserCreatedBy = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().CreatedBy;
                                uapobj.UserCreatedDate = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().CreatedDate;
                                uapobj.UserId = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().UID;
                            }
                            else
                            {
                                uapobj.Permission = Enums.UserActivityPermissionType.No.ToString();
                            }
                        }
                        userActivityPermissionList.Add(uapobj);
                    }

                    //// Start - Added by Sohel Pathan on 15/01/2015 for PL ticket #1139
                    //// Get list of custom fields
                    string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                    string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
                    List<CustomField> lstCustomField = new List<CustomField>();
                    lstCustomField = db.CustomFields.Where(customField => customField.ClientId == Sessions.User.CID && customField.IsDeleted.Equals(false) &&
                                                                                customField.EntityType.Equals(EntityTypeTactic) && customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                customField.IsRequired.Equals(true)).Select(customField => customField).ToList();

                    List<int> lstCustomFieldId = new List<int>();
                    List<CustomFieldOption> lstCustomFieldOption = new List<CustomFieldOption>();

                    if (lstCustomField.Count > 0)
                    {
                        //// Sort custom fields by name
                        lstCustomField = lstCustomField.OrderBy(customField => customField.Name).ToList();

                        //// Get list of Custom Field Ids
                        lstCustomFieldId = lstCustomField.Select(customField => customField.CustomFieldId).Distinct().ToList();

                        //// Get list of custom field options
                        lstCustomFieldOption = db.CustomFieldOptions.Where(customFieldOption => lstCustomFieldId.Contains(customFieldOption.CustomFieldId) && customFieldOption.IsDeleted == false)
                                                                    .Select(customFieldOption => customFieldOption).ToList();
                    }

                    //// Set data to Custom_Restriction model for Custom Fields.
                    List<CustomRestrictionModel> customRestrictionList = new List<CustomRestrictionModel>();
                    if (lstCustomFieldOption.Count > 0)
                    {
                        List<int> customFieldIdFromOptions = new List<int>();
                        customFieldIdFromOptions = lstCustomFieldOption.Select(option => option.CustomFieldId).Distinct().ToList();
                        if (customFieldIdFromOptions.Count() > 0)
                        {
                            lstCustomField = lstCustomField.Where(customField => customFieldIdFromOptions.Contains(customField.CustomFieldId)).ToList();
                        }

                        //// Sort custom field option list by custom field id and value
                        lstCustomFieldOption = lstCustomFieldOption.OrderBy(customFieldOption => customFieldOption.CustomFieldId).ThenBy(customFieldOption => customFieldOption.Value).ToList();

                        CustomRestrictionModel objCustomRestrictionModel;
                        foreach (var item in lstCustomFieldOption)
                        {
                            objCustomRestrictionModel = new CustomRestrictionModel();
                            objCustomRestrictionModel.Title = item.Value;
                            objCustomRestrictionModel.CustomField = item.CustomFieldId.ToString();
                            objCustomRestrictionModel.CustomFieldId = item.CustomFieldOptionId.ToString();
                            var IsUserRestrictionExist = userCustomRestrictionList != null ? userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == item.CustomFieldId && customRestriction.CustomFieldOptionId == item.CustomFieldOptionId).FirstOrDefault() : null;
                            if (IsUserRestrictionExist != null)
                            {
                                string permission = ((Enums.CustomRestrictionPermission)Enum.Parse(typeof(Enums.CustomRestrictionPermission), IsUserRestrictionExist.Permission.ToString())).ToString();
                                objCustomRestrictionModel.permissiontext = Enums.CustomRestrictionValues.FirstOrDefault(customRestriction => customRestriction.Key.Equals(permission)).Value;
                                objCustomRestrictionModel.Permission = IsUserRestrictionExist.Permission;
                            }
                            else
                            {
                                Enums.CustomRestrictionPermission defaultPermission = Common.GetDefaultCustomRestrictionType();
                                objCustomRestrictionModel.permissiontext = Enums.CustomRestrictionValues.FirstOrDefault(customRestriction => customRestriction.Key.Equals(defaultPermission.ToString())).Value;
                                objCustomRestrictionModel.Permission = (int)defaultPermission;
                            }
                            customRestrictionList.Add(objCustomRestrictionModel);
                        }
                    }

                    List<CustomDashboardModel> lstCustomDashboardld = new List<CustomDashboardModel>();
                    CustomDashboard cd = new CustomDashboard();
                    lstCustomDashboardld = cd.GetCustomDashboardsClientwise(UserId, Sessions.User.CID);
                    ViewData["CustomDashboardList"] = lstCustomDashboardld;

                    //// Set viewbag of Custom field list, to be used in view
                    ViewData["CustomFieldList"] = lstCustomField;

                    //// End - Added by Sohel Pathan on 15/01/2015 for PL ticket #1139

                    //// Set custom restriction list in viewbag, to be used in view
                    ViewData["CustomRestriction"] = customRestrictionList;
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View("ViewEditPermission", userActivityPermissionList);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav
        /// update user's activity permissions.
        /// <param name="permissionIds">comma separated permissionIds.</param>
        /// <param name="userId"></param>
        /// <param name="LoginId"></param>
        /// </summary>
        /// <returns>Return status flag value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveUserPermission(string permissionIds, Guid UserGuid, Guid LoginId = new Guid())// change type of parameter UserId from interger to guid #2953 
        {
            //// Start - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
            int userId = 0;
                if (!Sessions.User.UserId.Equals(LoginId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#', status = false }, JsonRequestBehavior.AllowGet);
                }
            
            //// End - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

            try
            {
                string[] arrPermissionId = permissionIds.Split(',');
                int CurrentUserID = Sessions.User.ID;
                //called method to get interger userid from GUID
                userId = Common.GetIntegerUserId(UserGuid);
                //// Start - Added/Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139
                //// Save User Activity Permissions
                if (userId != 0)
                {
                    //first, let's check if the user being managed belong to the same client as managing user for security reason -zz
                    ValidateUser(userId);
                    List<string> activityPermissions = arrPermissionId.Where(permission => (permission.ToLower().Contains("yes") || permission.ToLower().Contains("no"))).ToList();
                    int activityPermissionsResult = 0;
                    if (activityPermissions.Count > 0)
                    {
                        activityPermissionsResult = objBDSServiceClient.AddUserActivityPermissionsEx(userId, CurrentUserID, activityPermissions, Sessions.ApplicationId);
                    }
                    else
                    {
                        activityPermissionsResult = 1;
                    }

                    //// Save user custom restrictions
                    List<string> customRestrictionPermissions = arrPermissionId.Where(permission => !(permission.ToLower().Contains("yes") || permission.ToLower().Contains("no") || permission.ToLower().Contains("custdash"))).ToList();
                    int customRestrictionResult = 0;
                    if (customRestrictionPermissions.Count > 0)
                    {
                        customRestrictionResult = AddUserCustomRestrictions(userId, customRestrictionPermissions);
                    }
                    else
                    {
                        customRestrictionResult = 1;
                    }

                    List<string> CustDashPermissions = arrPermissionId.Where(permission => (permission.ToLower().Contains("custdash"))).ToList();
                    int customDashResult = 0;
                    if (CustDashPermissions.Count > 0)
                    {
                        customDashResult = AddUserCustomDashboard(userId, CustDashPermissions);
                    }
                    else
                    {
                        customDashResult = 1;
                    }

                    if (activityPermissionsResult >= 1 && customRestrictionResult >= 1 && customDashResult >= 1)
                    {
                        return Json(new { status = true }, JsonRequestBehavior.AllowGet);   //// Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                    }
                }
                //// End - Added/Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);  //// Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
        }

        private  void ValidateUser(int UserId)
        {
            User u = objBDSServiceClient.GetUserDetailsByIdEx(UserId);
            if (u == null || u.CID != Sessions.User.CID) throw new Exception(string.Format("UserId: {0} not valid", UserId));
        }    

        #endregion

        #region "Other"
        /// <summary>
        /// Added By: Mitesh Vaishnav
        /// Reset user's activity permissions as per his role default permission.
        /// <param name="userId"></param>
        /// <param name="LoginId"></param>
        /// </summary>
        /// <returns>Return status flag value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResetToRoleDefault(Guid UserGuid, Guid LoginId = new Guid())// change type of parameter UserId from interger to guid #2953 
        {
            // Start - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
            int userId = 0;
                if (Sessions.User.UserId != LoginId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            
            // End - Added by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check

            try
            {
                userId = Common.GetIntegerUserId(UserGuid);//called method to get interger userid from GUID
                ValidateUser(userId);
                int creatorId = Sessions.User.ID;

                //// Reset Default Role settings.
                int result = objBDSServiceClient.ResetToRoleDefaultEx(userId, creatorId, Sessions.ApplicationId);
                if (result >= 1)
                {
                    TempData["SuccessMessage"] = Common.objCached.UserPermissionsResetToDefault;
                    return Json(new { status = true }, JsonRequestBehavior.AllowGet);   // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);  // Modified by Sohel Pathan on 11/07/2014 for Internal Functional Review Points #53 to implement user session check
        }
        #endregion

        #region User defined functions

        #region Add user custom restriction.
        /// <summary>
        /// Function to Add user custom restrictions
        /// Previous data will be deleted, if any exists, prior to adding new data
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="customFieldPermissions">string list of custom field permissions</param>
        /// <returns>Return 1 if success otherwise 0</returns>
        public int AddUserCustomRestrictions(int userId, List<string> customFieldPermissions)
        {
            int returnValue = 0;

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //// Retrieve previous custom restrictions and delete it.
                    List<Models.CustomRestriction> userCustomrestrictionList = db.CustomRestrictions.Where(usr => usr.UserId == userId).ToList();
                    if (userCustomrestrictionList.Count > 0)
                    {
                        foreach (var customRestriction in userCustomrestrictionList)
                        {
                            db.Entry(customRestriction).State = EntityState.Deleted;
                            db.CustomRestrictions.Remove(customRestriction);
                        }
                    }
                    else
                    {
                        returnValue = 1;
                    }
                    string[] splitpermissions;
                    Models.CustomRestriction objCustomRestriction;
                    foreach (var permission in customFieldPermissions)
                    {
                        splitpermissions = permission.Split('_');
                        objCustomRestriction = new Models.CustomRestriction();
                        objCustomRestriction.UserId = userId;
                        objCustomRestriction.Permission = Convert.ToInt16(splitpermissions[0]);
                        objCustomRestriction.CustomFieldId = Convert.ToInt32(splitpermissions[1]);
                        objCustomRestriction.CustomFieldOptionId = Convert.ToInt32(splitpermissions[2]);
                        objCustomRestriction.CreatedDate = System.DateTime.Now;
                        objCustomRestriction.CreatedBy = Sessions.User.ID;
                        db.Entry(objCustomRestriction).State = EntityState.Added;
                        db.CustomRestrictions.Add(objCustomRestriction);
                    }

                    int result = db.SaveChanges();
                    if (result > 0)
                    {
                        returnValue = 1;
                    }

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    returnValue = 0;
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return returnValue;
        }
        #endregion

        #region Add user custom Dashboards.
        /// <summary>
        /// Function to Add user custom restrictions
        /// Previous data will be deleted, if any exists, prior to adding new data
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="customDashPermissions">string list of custom field permissions</param>
        /// <returns>Return 1 if success otherwise 0</returns>
        public int AddUserCustomDashboard(int userId, List<string> customDashPermissions)
        {
            int DashreturnValue = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //// Retrieve previous custom restrictions and delete it.
                    List<Models.User_Permission> userCustomDashboardList = db.User_Permission.Where(usr => usr.UserId == userId).ToList();
                    if (userCustomDashboardList.Count > 0)
                    {
                        foreach (var customDashboard in userCustomDashboardList)
                        {
                            db.Entry(customDashboard).State = EntityState.Deleted;
                            db.User_Permission.Remove(customDashboard);
                        }
                    }
                    else
                    {
                        DashreturnValue = 1;
                    }
                    string[] splitpermissions;
                    Models.User_Permission objCustomDashboard;
                    foreach (var permission in customDashPermissions)
                    {
                        splitpermissions = permission.Split('_');

                        if (splitpermissions != null && splitpermissions.Length > 0 && splitpermissions[1] != "None")
                        {
                            objCustomDashboard = new Models.User_Permission();
                            objCustomDashboard.DashboardId = Convert.ToInt16(splitpermissions[2]);
                            objCustomDashboard.ApplicationActivityId = null;
                            objCustomDashboard.UserId = userId;
                            objCustomDashboard.CreatedDate = System.DateTime.Now;
                            objCustomDashboard.CreatedBy = Sessions.User.ID;
                            objCustomDashboard.PermissionType = splitpermissions[1] == "None" ? null : splitpermissions[1];
                            objCustomDashboard.HomePageId = null;
                            db.Entry(objCustomDashboard).State = EntityState.Added;
                            db.User_Permission.Add(objCustomDashboard);
                        }
                    }

                    int result = db.SaveChanges();
                    if (result > 0)
                    {
                        DashreturnValue = 1;
                    }

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    DashreturnValue = 0;
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
            return DashreturnValue;
        }
        #endregion

       
        #endregion
    }
}
