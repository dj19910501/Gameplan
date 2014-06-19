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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Hierarchy()
        {
            List<OrganizationModel> listorganizationmodel = new List<OrganizationModel>();

            try
            {
                string systemadmin = Enums.RoleCodes.SA.ToString();
                string clientadmin = Enums.RoleCodes.CA.ToString();
                string director = Enums.RoleCodes.D.ToString();
                string planner = Enums.RoleCodes.P.ToString();

                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();
                var memberlist = bdsuserrepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
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

        public ActionResult ManageRoles()
        {
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
                objrole.Description = roledesc;
                Session["session"] = objrole;

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


        public ActionResult Edit(Guid roleId)
        {
            try
            {
                BDSService.BDSServiceClient bdsuserrepository = new BDSServiceClient();
                var rolelist = bdsuserrepository.GetAllRoleList(Sessions.ApplicationId);
                if (roleId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    var prevrole = (Role)Session["session"];
                    if (prevrole != null)
                    {
                        ViewBag.current = prevrole.Description;
                    }
                    ViewBag.roleid = roleId;
                }
                else
                {
                    ViewBag.current = rolelist.Where(role => role.RoleId == roleId).Select(role => role.Description).FirstOrDefault();
                    ViewBag.roleid = roleId;
                    ViewBag.colorcode = rolelist.Where(role => role.RoleId == roleId).Select(role => role.ColorCode).FirstOrDefault();
                }
                List<RoleModel> listrolemodel = new List<RoleModel>();
                IList<SelectListItem> RoleList = new List<SelectListItem>();
                RoleList = rolelist.Select(role => new SelectListItem() { Text = role.Title, Value = role.RoleId.ToString(), Selected = false })
                                .OrderBy(it => it.Text).ToList();
                var activitylist = bdsuserrepository.GetApplicationactivitylist(Sessions.ApplicationId);
                var filterlist = bdsuserrepository.GetRoleactivitypermissions(roleId);
                ViewData["permissionlist"] = filterlist;
                ViewData["activitylist"] = activitylist;

                TempData["RoleList"] = new SelectList(RoleList, "Value", "Text", RoleList.First());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return View();
        }

        public ActionResult Delete(Guid roleid, string selectedrole)
        {

            ViewData["users"] = objBDSServiceClient.GetRoleMemberList(Sessions.ApplicationId, roleid);
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
                var rolelistData = listrolemodel.Select(role => new SelectListItem()
                {
                    Text = role.RoleTitle,
                    Value = Convert.ToString(role.RoleId),
                }).OrderBy(t => t.Text);

                ViewData["roles"] = rolelistData;
                ViewData["roleselected"] = selectedrole;
                ViewData["deleterole"] = roleid;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return PartialView("Delete");
        }


        [HttpPost]
        public JsonResult DeleteRole(Guid delroleid, Guid reassignroleid)
        {
            int retval = objBDSServiceClient.DeleteRoleAndReassign(delroleid, reassignroleid, Sessions.ApplicationId);
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
        public JsonResult Save(string roledesc, string checkbox, string colorcode, Guid roleid)
        {
            string permissionID = checkbox.ToString();

            int retval = objBDSServiceClient.CreateRole(roledesc, permissionID, colorcode, Sessions.ApplicationId, Sessions.User.UserId, roleid);
            if (retval == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            //return Json(false, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CopyRole(string copyroledesc, Guid originalroleid)
        {
            if (copyroledesc != null && copyroledesc != string.Empty)
            {
                BDSService.Role objrole = new BDSService.Role();
                objrole.Description = copyroledesc;
                Session["session"] = objrole;

                int retval = objBDSServiceClient.CopyRole(copyroledesc, originalroleid, Sessions.ApplicationId, Sessions.User.UserId);
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
    }
}
