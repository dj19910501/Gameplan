using Elmah;
using Newtonsoft.Json;
using RestSharp.Serializers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace RevenuePlanner.Controllers
{
    public class ColumnViewController : Controller
    {
        //
        // GET: /ColumnView/
        StoredProcedure objSp = new StoredProcedure();
        IColumnView objcolumnView = new ColumnView();
        private MRPEntities db = new MRPEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region column Management Add custom fields in plan grid
        #region method to bind attributes for user selection #2590
        public ActionResult GetAttributeList_ColumnView(bool IsGrid = true)
        {
            List<ColumnViewEntity> allattributeList = new List<ColumnViewEntity>();

            bool IsSelectall = false;
            string attributexml = string.Empty;
            List<CustomAttribute> BasicFields = new List<CustomAttribute>();
            try
            {


                ViewBag.IsGrid = IsGrid;
                allattributeList = objcolumnView.GetCutomefieldModel(Sessions.User.ClientId, IsGrid, out IsSelectall);
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



                int viewId = objcolumnView.SaveColumnView(Sessions.User.UserId, ViewName, AttributeDetail, Isgrid);

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
