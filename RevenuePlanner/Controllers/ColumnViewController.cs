using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class ColumnViewController : CommonController
    {
        // GET: /ColumnView/
        IColumnView objcolumnView = new ColumnView();
        #region column Management Add custom fields in plan grid
        #region method to bind attributes for user selection #2590
        public ActionResult GetAttributeList_ColumnView(bool IsGrid = true)
        {
            List<ColumnViewEntity> allattributeList = new List<ColumnViewEntity>();
            bool IsSelectall = false;
            try
            {
                ViewBag.IsGrid = IsGrid;
                allattributeList = objcolumnView.GetCustomfieldModel(Sessions.User.CID, IsGrid, out IsSelectall, Sessions.User.ID);
                ViewBag.IsSelectAll = IsSelectall;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return PartialView("_AddColumnView", allattributeList);
        }
        #endregion

        #region method to  save column view

        [HttpPost]
        public JsonResult SaveColumnView(List<AttributeDetail> AttributeDetail, string ViewName = null, bool Isgrid = true)
        {

            try
            {
                int viewId = objcolumnView.SaveColumnView(Sessions.User.ID, ViewName, AttributeDetail, Isgrid);
                if (viewId > 0)
                {
                    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion
        #endregion




    }
}
