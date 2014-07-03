using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RevenuePlanner.Models;
using RevenuePlanner.BDSService;
using Elmah;
using RevenuePlanner.Helpers;

namespace RevenuePlanner.Controllers
{
    public class OrganizationController : CommonController
    {
        //
        // GET: /Organization/

        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        private MRPEntities db = new MRPEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Hierarchy()
        {
            List<OrganizationModel> listorganizationmodel = new List<OrganizationModel>();

            try
            {
                //string systemadmin = Enums.RoleCodes.SA.ToString();
                //string clientadmin = Enums.RoleCodes.CA.ToString();
                //string director = Enums.RoleCodes.D.ToString();
                //string planner = Enums.RoleCodes.P.ToString();

                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();
                var memberlist = bdsuserrepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);//Sessions.IsSystemAdmin);
                foreach (var item in memberlist)
                {
                    OrganizationModel obj = new OrganizationModel();
                    obj.UserId = item.UserId;
                    obj.GeographyId = item.GeographyId;
                    obj.Client = item.Client;
                    obj.ClientId = item.ClientId;
                    obj.DisplayName = item.DisplayName;
                    obj.Email = item.Email;
                    obj.FirstName = item.FirstName;
                    obj.RoleCode = item.RoleCode;
                    obj.RoleId = item.RoleId;
                    obj.RoleTitle = item.RoleTitle;
                    obj.LastName = item.LastName;
                    obj.BusinessUnitId = item.BusinessUnitId;
                    obj.JobTitle = item.JobTitle;
                    listorganizationmodel.Add(obj);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(listorganizationmodel.AsEnumerable());
        }

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
                var memberlist = bdsuserrepository.GetAllRoleList(Sessions.ApplicationId);
                foreach (var item in memberlist)
                {
                    RoleModel role = new RoleModel();
                    role.RoleTitle = item.Title;
                    role.RoleCode = item.Code;
                    role.RoleId = item.RoleId;
                    listrolemodel.Add(role);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(listrolemodel.AsEnumerable());
        }

        [HttpPost]
        public JsonResult AddRole(string roledesc)
        {
            if (roledesc != null && roledesc != string.Empty)
            {
                BDSService.Role objrole = new BDSService.Role();
                objrole.Description = roledesc.Trim();
                objrole.Title = roledesc.Trim();
                //Session["session"] = objrole;commented by uday for functional review point...3-7-2014
                TempData["objrole"] = objrole;
                int retval = objBDSServiceClient.DuplicateRoleCheck(objrole, Sessions.ApplicationId);
                if (retval == 1)
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            return Json(false);
            //return Json(new { name = "dsfsdf" });
            //return RedirectToAction("Edit", "Organization");
        }

        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult RoleEdit(Guid roleId)//changed by uday for functional review point...3-7-2014;
        {
            // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            try
            {
                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();
                var rolelist = bdsuserrepository.GetAllRoleList(Sessions.ApplicationId);
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

                    rolelist = rolelist.Where(a => a.RoleId != roleId).ToList();//review point
                }
                List<RoleModel> listrolemodel = new List<RoleModel>();
                IList<SelectListItem> RoleList = new List<SelectListItem>();
                RoleList = rolelist.Select(role => new SelectListItem() { Text = role.Title, Value = role.RoleId.ToString(), Selected = false })
                                .OrderBy(it => it.Text).ToList();
                var activitylist = bdsuserrepository.GetApplicationactivitylist(Sessions.ApplicationId);
                var filterlist = bdsuserrepository.GetRoleactivitypermissions(roleId);
                ViewData["permissionlist"] = filterlist;
                ViewData["activitylist"] = activitylist;
                string ids = string.Empty;
                //for (int i = 0; i < filterlist.Count; i++)
                //{
                //    if (i == 0)
                //    {
                //        ids += "role_" + filterlist[i].ApplicationActivityId.ToString();
                //    }
                //    else
                //    {
                //        ids += ',' + "role_" + filterlist[i].ApplicationActivityId.ToString();
                //    }
                //}commented by uday for functional review point...3-7-2014
                if (filterlist.Count > 0)
                {
                    ids = string.Join(",", filterlist.Select(filter => filter.ApplicationActivityId.ToString()).ToArray());
                }
                ViewData["permissionids"] = ids.ToString();
                TempData["RoleList"] = new SelectList(RoleList, "Value", "Text", RoleList.First());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return View();
        }

        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult RoleDelete(Guid roleid, string selectedrole)
        {

            ViewData["users"] = objBDSServiceClient.GetRoleMemberList(Sessions.ApplicationId, roleid);
            //changed by uday for functional review point...3-7-2014;
            try
            {
                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();
                
                var rolelistData = bdsuserrepository.GetAllRoleList(Sessions.ApplicationId).Where(rolelist => rolelist.RoleId != roleid).ToList().Select(role => new SelectListItem()
                {
                    Text = role.Title,
                    Value = Convert.ToString(role.RoleId),
                }).OrderBy(role => role.Text);

                ViewData["roles"] = rolelistData;
                ViewData["roleselected"] = selectedrole;
                ViewData["deleterole"] = roleid;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return PartialView("RoleDelete");//name change by uday for functional review point...3-7-2014
        }

        public ActionResult CheckPermission(Guid roleid, string permission)
        {
            List<User> user_list = new List<User>();
            var user_role_mapping = objBDSServiceClient.GetRoleMemberList(Sessions.ApplicationId, roleid);
            //var user_activity_mapping = objBDSServiceClient.GetUserActivityPermission(Sessions.ApplicationId,)
            var activitylist = objBDSServiceClient.GetApplicationactivitylist(Sessions.ApplicationId);
            List<int> idsList = new List<int>();
            foreach (string id in permission.Split(','))
            {
                idsList.Add(Convert.ToInt32(id));
            }

            var activity_CodeList = activitylist.Where(activity => idsList.Contains(activity.ApplicationActivityId)).ToList();
            try
            {

                foreach (var user_role_map in user_role_mapping)
                {
                    List<string> user_activity_mapping = objBDSServiceClient.GetUserActivityPermission(user_role_map.UserId, Sessions.ApplicationId);
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
            }

            return PartialView("CheckPermission");
        }

        [HttpPost]
        public JsonResult DeleteRole(Guid delroleid, Guid? reassignroleid)
        {
            if (reassignroleid == null)
            {
                reassignroleid = Guid.Empty;
            }
            int retval = objBDSServiceClient.DeleteRoleAndReassign(delroleid, reassignroleid.Value, Sessions.ApplicationId, Sessions.User.UserId);
            if (retval == 1)
            {
                TempData["SuccessMessage"] = "Role Deleted Successfully.";
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(string roledesc, string checkbox, string colorcode, Guid roleid, string delpermission)
        {
            string permissionID = checkbox.ToString();

            int retval = objBDSServiceClient.CreateRole(roledesc, permissionID, colorcode, Sessions.ApplicationId, Sessions.User.UserId, roleid, delpermission);
            if (retval == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult CopyRole(string copyroledesc, Guid originalroleid)
        {
            if (copyroledesc != null && copyroledesc != string.Empty)
            {
                BDSService.Role objrole = new BDSService.Role();
                objrole.Description = copyroledesc;
                objrole.Title = copyroledesc.Trim();
                Session["session"] = objrole;
                int retvalcheck = objBDSServiceClient.DuplicateRoleCheck(objrole, Sessions.ApplicationId);
                if (retvalcheck == 1)
                {
                    int retval = objBDSServiceClient.CopyRole(copyroledesc.Trim(), originalroleid, Sessions.ApplicationId, Sessions.User.UserId);
                    if (retval == 1)
                    {
                        TempData["SuccessMessage"] = "Role Copied Successfully.";
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false);
        }

        /// <summary>
        /// Organization Hierarchy
        /// </summary>
        /// <returns></returns>
        public ActionResult OrganizationHierarchy()
        {
            // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            try
            {
                List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
                lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);

                List<BDSService.UserHierarchy> lstManagerUserHierarchy = lstUserHierarchy.Where(u => u.ManagerId == null).ToList();

                var result = lstManagerUserHierarchy.Select(u => CreateUserHierarchy(lstUserHierarchy, u)).ToList();
                ViewBag.UserHierarchy = result;

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
        /// <returns></returns>
        public List<BDSService.UserHierarchy> GetSubUsers(List<BDSService.UserHierarchy> lstUserHierarchy, Guid? managerId)
        {
            if (managerId == null)
                return null;
            else
                return lstUserHierarchy.Where(u => u.ManagerId == managerId).ToList();
        }

        /// <summary>
        /// Function for making user hierarchy
        /// Dharmraj, Ticket #520
        /// </summary>
        /// <param name="lstUserHierarchy"></param>
        /// <param name="objUserHierarchy"></param>
        /// <returns></returns>
        public UserHierarchyModel CreateUserHierarchy(List<BDSService.UserHierarchy> lstUserHierarchy, BDSService.UserHierarchy objUserHierarchy)
        {
            Guid userid = objUserHierarchy.UserId;
            string FirstName = objUserHierarchy.FirstName;
            string LastName = objUserHierarchy.LastName;
            string Email = objUserHierarchy.Email;
            Guid RoleId = objUserHierarchy.RoleId;
            string RoleTitle = objUserHierarchy.RoleTitle;
            string ColorCode = objUserHierarchy.ColorCode;
            string JobTitle = objUserHierarchy.JobTitle;
            Guid GeographyId = objUserHierarchy.GeographyId;
            var objGeography = db.Geographies.FirstOrDefault(g => g.GeographyId == objUserHierarchy.GeographyId);
            string Geography = objGeography == null ? string.Empty : objGeography.Title;
            string Phone = objUserHierarchy.Phone;
            Guid? ManagerId = objUserHierarchy.ManagerId;
            var subUsers = GetSubUsers(lstUserHierarchy, userid)
              .Select(r => CreateUserHierarchy(lstUserHierarchy, r))
              .ToList();
            return new UserHierarchyModel { UserId = userid, FirstName = FirstName, LastName = LastName, Email = Email, RoleId = RoleId, RoleTitle = RoleTitle, ColorCode = ColorCode, JobTitle = JobTitle, GeographyId = GeographyId, Geography = Geography, Phone = Phone, ManagerId = ManagerId, subUsers = subUsers };
        }
        /// <summary>
        /// Added By: Mitesh Vaishnav for PL ticket #521
        /// View/Edit user permission.
        /// </summary>
        public ActionResult ViewEditPermission(string Id, string Mode)
        {
            // Start - Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            if ((bool)ViewBag.IsUserAdminAuthorized == false && Mode.ToLower() != Enums.UserPermissionMode.View.ToString().ToLower() && Mode.ToLower() != Enums.UserPermissionMode.MyPermission.ToString().ToLower())
            {
                return RedirectToAction("Index", "NoAccess");
            }
            // End - Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic

            ViewBag.PermissionMode = Mode;
            Guid UserId = Guid.Parse(Id);
            ViewBag.userId = UserId;
            var userDetails = objBDSServiceClient.GetTeamMemberDetails(UserId, Sessions.ApplicationId);
            var roleColorCode = objBDSServiceClient.GetAllRoleList(Sessions.ApplicationId).Where(rol => rol.RoleId == userDetails.RoleId).FirstOrDefault().ColorCode;
            ViewBag.RoleColorCode = roleColorCode;
            ViewBag.Name = userDetails.FirstName + " " + userDetails.LastName;
            ViewBag.RoleName = userDetails.RoleTitle;
            var clientVerticals = db.Verticals.Where(ver => ver.ClientId == Sessions.User.ClientId).ToList();
            var clientGeography = db.Geographies.Where(geo => geo.ClientId == Sessions.User.ClientId).ToList();
            var clientBusinessUnit = db.BusinessUnits.Where(bu => bu.ClientId == Sessions.User.ClientId).ToList();
            var userCustomRestrictionList = objBDSServiceClient.GetUserCustomRestrictionList(UserId, Sessions.ApplicationId);
            var allAtctivity = objBDSServiceClient.GetAllApplicationActivity(Sessions.ApplicationId);
            var userActivity = objBDSServiceClient.GetUserActivity(UserId, Sessions.ApplicationId);

            List<UserActivityPermissionModel> userActivityPermissionList = new List<UserActivityPermissionModel>();
            foreach (var item in allAtctivity)
            {
                UserActivityPermissionModel uapobj = new UserActivityPermissionModel();
                uapobj.ApplicationActivityId = item.ApplicationActivityId;
                uapobj.ApplicationId = item.ApplicationId;
                uapobj.CreatedDate = item.CreatedDate;
                uapobj.ParentId = item.ParentId;
                uapobj.Title = item.ActivityTitle;
                uapobj.Permission = Enums.UserActivityPermissionType.No.ToString();
                if (userActivity != null)
                {
                    if (userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).ToList().Count > 0)
                    {
                        uapobj.Permission = Enums.UserActivityPermissionType.Yes.ToString();
                        uapobj.UserCreatedBy = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().CreatedBy;
                        uapobj.UserCreatedDate = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().CreatedDate;
                        uapobj.UserId = userActivity.Where(uact => uact.ApplicationActivityId == item.ApplicationActivityId).FirstOrDefault().UserId;
                    }
                    else
                    {
                        uapobj.Permission = Enums.UserActivityPermissionType.No.ToString();
                    }
                }
                userActivityPermissionList.Add(uapobj);
            }
            List<CustomRestrictionModel> customRestrictionList = new List<CustomRestrictionModel>();
            foreach (var item in clientVerticals)
            {
                CustomRestrictionModel cRestrictionobj = new CustomRestrictionModel();
                cRestrictionobj.Title = item.Title;
                cRestrictionobj.CustomField =Enums.CustomRestrictionType.Verticals.ToString();
                cRestrictionobj.CustomFieldId = item.VerticalId.ToString();
                var IsUserRestrictionExist = userCustomRestrictionList != null ? userCustomRestrictionList.Where(ucr => ucr.CustomFieldId == item.VerticalId.ToString() && ucr.CustomField==Enums.CustomRestrictionType.Verticals.ToString()).FirstOrDefault() : null;
                if (IsUserRestrictionExist != null)
                {
                    string permission = ((Enums.CustomRestrictionPermission)Enum.Parse(typeof(Enums.CustomRestrictionPermission), IsUserRestrictionExist.Permission.ToString())).ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(permission)).Value;
                    cRestrictionobj.Permission = IsUserRestrictionExist.Permission;
                }
                else
                {
                    string none = Enums.CustomRestrictionPermission.None.ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(none)).Value;
                    cRestrictionobj.Permission = (int)Enums.CustomRestrictionPermission.None;
                }
                customRestrictionList.Add(cRestrictionobj);
            }
            foreach (var item in clientGeography)
            {
                CustomRestrictionModel cRestrictionobj = new CustomRestrictionModel();
                cRestrictionobj.Title = item.Title;
                cRestrictionobj.CustomField = Enums.CustomRestrictionType.Geography.ToString();
                cRestrictionobj.CustomFieldId = item.GeographyId.ToString();
                var IsUserRestrictionExist = userCustomRestrictionList != null ? userCustomRestrictionList.Where(ucr => ucr.CustomFieldId == item.GeographyId.ToString() && ucr.CustomField==Enums.CustomRestrictionType.Geography.ToString()).FirstOrDefault() : null;
                if (IsUserRestrictionExist != null)
                {
                    string permission = ((Enums.CustomRestrictionPermission)Enum.Parse(typeof(Enums.CustomRestrictionPermission), IsUserRestrictionExist.Permission.ToString())).ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(permission)).Value;
                    cRestrictionobj.Permission = IsUserRestrictionExist.Permission;
                }
                else
                {
                    string none = Enums.CustomRestrictionPermission.None.ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(none)).Value;
                    cRestrictionobj.Permission = (int)Enums.CustomRestrictionPermission.None;
                }
                customRestrictionList.Add(cRestrictionobj);
            }
            foreach (var item in clientBusinessUnit)
            {
                CustomRestrictionModel cRestrictionobj = new CustomRestrictionModel();
                cRestrictionobj.Title = item.Title;
                cRestrictionobj.CustomField = Enums.CustomRestrictionType.BusinessUnit.ToString();
                cRestrictionobj.CustomFieldId = item.BusinessUnitId.ToString();
                var IsUserRestrictionExist = userCustomRestrictionList != null ? userCustomRestrictionList.Where(ucr => ucr.CustomFieldId == item.BusinessUnitId.ToString() && ucr.CustomField==Enums.CustomRestrictionType.BusinessUnit.ToString()).FirstOrDefault() : null;
                if (IsUserRestrictionExist != null)
                {
                    string permission = ((Enums.CustomRestrictionPermission)Enum.Parse(typeof(Enums.CustomRestrictionPermission), IsUserRestrictionExist.Permission.ToString())).ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(permission)).Value;
                    cRestrictionobj.Permission = IsUserRestrictionExist.Permission;
                }
                else
                {
                    string none = Enums.CustomRestrictionPermission.None.ToString();
                    cRestrictionobj.permissiontext = Enums.CustomRestrictionValues.Single(customRestriction => customRestriction.Key.Equals(none)).Value;
                    cRestrictionobj.Permission = (int)Enums.CustomRestrictionPermission.None;
                }
                customRestrictionList.Add(cRestrictionobj);
            }
            ViewData["CustomRestriction"] = customRestrictionList;
            return View(userActivityPermissionList);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav
        /// update user's activity permissions.
        /// </summary>
        [HttpPost]
        public bool SaveUserPermission(string permissionIds, string userId)
        {
            try
            {
                string[] arrPermissionId = permissionIds.Split(',');
                Guid UserId = Guid.Parse(userId);
                Guid CurrentUserID = Sessions.User.UserId;
                int i = objBDSServiceClient.AddUserActivityPermissions(UserId, CurrentUserID, arrPermissionId.ToList(), Sessions.ApplicationId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav
        /// Reset user's activity permissions as per his role default permission.
        /// </summary>
        [HttpPost]
        public bool ResetToRoleDefault(string userId)
        {
            try
            {
                Guid UserId = Guid.Parse(userId);
                Guid creatorId = Sessions.User.UserId;
                int i = objBDSServiceClient.resetToRoleDefault(UserId, creatorId, Sessions.ApplicationId);
                TempData["Successmessage"] = "Model inputs successfully saved.";
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
