using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult GetAttributeList_ColumnView()
        {
            List<ColumnViewEntity> allattributeList = new List<ColumnViewEntity>();
            List<string> SelectedCustomfieldID = new List<string>();
            bool IsSelectall=false;
            try
            {
                var userview = db.User_CoulmnView.Where(a => a.CreatedBy == Sessions.User.UserId).FirstOrDefault();
                if (userview == null)
                    IsSelectall = true;
                else
                {
                    SelectedCustomfieldID = userview.User_CoulmnView_attribute.Select(a => a.AttributeId).ToList();
                }
                ViewBag.IsSelectAll = IsSelectall;
                DataTable dtColumnAttribute = objcolumnView.GetCustomFieldList(Sessions.User.ClientId);
                if (dtColumnAttribute != null && dtColumnAttribute.Rows.Count > 0)
                {

                    var columnattribute = dtColumnAttribute.AsEnumerable().Select(row => new
                    {
                        EntityType = Convert.ToString(row["EntityType"]),
                        CustomFieldId = Convert.ToString(row["CustomFieldId"]),
                        CutomfieldName = Convert.ToString(row["Name"]),
                        ParentId = Convert.ToInt32(row["ParentId"])

                    }).ToList();

                    var BasicFields = Enums.PlanGrid_Column.Select(row => new
                    {
                        EntityType = "Basic",
                        CustomFieldId = Convert.ToString(row.Key),
                        CutomfieldName = Convert.ToString(row.Value),
                        ParentId = 0
                    }).ToList();

                    List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                    string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();


                    var mqlfield = new
                    {
                        EntityType = "Basic",
                        CustomFieldId = "mql",
                        CutomfieldName = MQLTitle,
                        ParentId = 0
                    };
                    BasicFields.Add(mqlfield);
                    BasicFields.AddRange(columnattribute);
                    allattributeList = BasicFields.GroupBy(a => new { EntityType = a.EntityType }).Select(a => new ColumnViewEntity
                    {
                        EntityType = a.Key.EntityType,

                        AttributeList = BasicFields.Where(atr => atr.EntityType == a.Key.EntityType).Select(atr => new ColumnViewAttribute
                        {
                            CustomFieldId = atr.CustomFieldId,
                            CutomfieldName = atr.CutomfieldName,
                            ParentID = atr.ParentId,
                            IsChecked = SelectedCustomfieldID.Contains(atr.CustomFieldId)?true:false
                        }).ToList()
                    }).ToList();
                }
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
        public JsonResult SaveColumnView(List<AttributeDetail> AttributeDetail, string ViewName = null)
        {
            try
            {
                if (AttributeDetail != null)
                {
                    int viewId = objcolumnView.SaveColumnView(Sessions.User.UserId, ViewName);

                    if ( viewId != -1)
                    {
                        int result = objcolumnView.SaveColumnViewAttribute(viewId, AttributeDetail);
                        if (result > 0)
                        {
                            //if (ViewName != null)
                            //{
                            //    List<ViewByModel> viewByListResult = objSp.spViewByDropDownList(Convert.ToString(Sessions.PlanId));
                            //    return Json(new { Success = true, SuccessMessage = Common.objCached.SuccessColumnView, ViewById = viewByListResult }, JsonRequestBehavior.AllowGet);
                            //}
                            //else
                            //{
                                return Json(new { Success = true}, JsonRequestBehavior.AllowGet);
                            //}
                        }

                        else
                            return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

                    }
                    else
                        return Json(new { Success = false, ErrorMessage = Common.objCached.DuplicateColumnView }, JsonRequestBehavior.AllowGet);

                }

                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion




    }
}
