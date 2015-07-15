using BDSService.BDSEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDSService
{
    public class BDSMenuRepository
    {
        #region Variables

        private BDSAuthEntities db = new BDSAuthEntities();

        #endregion

        public List<Menu> GetMenu(Guid ApplicationId, Guid RoleId)
        {
            try
            {
                List<Menu> lstMenu = (from m in db.Menu_Application
                                       where m.IsDeleted == false && m.ApplicationId == ApplicationId && m.IsDisplayInMenu == true 
                                       orderby m.SortOrder
                                       select new Menu {
                                           MenuApplicationId = m.MenuApplicationId,
                                           ParentApplicationId = m.ParentApplicationId,
                                           Code = m.Code,
                                           Name = m.Name,
                                           Description = m.Description,
                                           IsDisplayInMenu = m.IsDisplayInMenu,
                                           SortOrder = m.SortOrder,
                                           ControllerName = m.ControllerName,
                                           ActionName = m.ActionName,
                                           IsEnable = false
                                       }).ToList();
                
                if (lstMenu != null && lstMenu.Count > 0)
                {
                    List<int> MenuApplicationIds = lstMenu.Select(m => m.MenuApplicationId).ToList();
                    List<Role_Permission> lstRolPermission = (from r in db.Role_Permission
                                                              where r.IsDeleted == false && r.RoleId == RoleId && MenuApplicationIds.Contains(r.MenuApplicationId) && r.PermissionCode != 0
                                                          select r).ToList();
                    foreach (Menu o in lstMenu)
                    {
                        if (lstRolPermission != null && lstRolPermission.Count > 0)
                        {
                            if (lstRolPermission.Where(r => r.MenuApplicationId == o.MenuApplicationId).FirstOrDefault() != null)
                            {
                                o.IsEnable = true;
                            }
                        }
                        
                    }
                }
                return lstMenu;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Menu> GetAllMenu(Guid ApplicationId, Guid RoleId)
        {
            try
            {
                List<Menu> lstMenu = (from m in db.Menu_Application
                                      where m.IsDeleted == false && m.ApplicationId == ApplicationId 
                                      orderby m.SortOrder
                                      select new Menu
                                      {
                                          MenuApplicationId = m.MenuApplicationId,
                                          ParentApplicationId = m.ParentApplicationId,
                                          Code = m.Code,
                                          Name = m.Name,
                                          Description = m.Description,
                                          IsDisplayInMenu = m.IsDisplayInMenu,
                                          SortOrder = m.SortOrder,
                                          ControllerName = m.ControllerName,
                                          ActionName = m.ActionName,
                                          IsEnable = false
                                      }).ToList();

                if (lstMenu != null && lstMenu.Count > 0)
                {
                    List<int> MenuApplicationIds = lstMenu.Select(m => m.MenuApplicationId).ToList();
                    List<Role_Permission> lstRolPermission = (from r in db.Role_Permission
                                                              where r.IsDeleted == false && r.RoleId == RoleId && MenuApplicationIds.Contains(r.MenuApplicationId) && r.PermissionCode != 0
                                                              select r).ToList();
                    foreach (Menu o in lstMenu)
                    {
                        if (lstRolPermission != null && lstRolPermission.Count > 0)
                        {
                            if (lstRolPermission.Where(r => r.MenuApplicationId == o.MenuApplicationId).FirstOrDefault() != null)
                            {
                                o.IsEnable = true;
                            }
                        }

                    }
                }
                return lstMenu;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
