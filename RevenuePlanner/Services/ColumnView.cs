using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace RevenuePlanner.Services
{
    public class ColumnView : IColumnView
    {
        private MRPEntities objDbMrpEntities;
        public ColumnView()
        {
            objDbMrpEntities = new MRPEntities();
        }
        #region method to fill custom field for create new view
        // Get Custom field
        public DataTable GetCustomFieldList(int ClientId)
        {

            DataTable datatable = new DataTable();

            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("sp_GetCustomFieldList", Connection);

            using (command)
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ClientId", ClientId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(datatable);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return datatable;

        }
        #endregion
        #region method to save ColumnView
        public int SaveColumnView(int UserId, string ViewName, List<AttributeDetail> AttributeDetail, bool Isgrid = true)
        {
            int result = 0;
            string xmlElements = string.Empty;
            try
            {
                User_CoulmnView columnview = new User_CoulmnView();
                if (!string.IsNullOrEmpty(ViewName))
                {
                    columnview = objDbMrpEntities.User_CoulmnView.Where(a => a.ViewName.ToLower() == ViewName.ToLower() && a.CreatedBy == UserId).FirstOrDefault();
                }
                else
                {
                    columnview = objDbMrpEntities.User_CoulmnView.Where(a => a.ViewName == null && a.CreatedBy == UserId).FirstOrDefault();
                }

                if (AttributeDetail != null)
                {
                    var AttributexmlElements = new XElement("ViewDetail", AttributeDetail.Select(i => new XElement("attribute",
                        new XAttribute("AttributeType", i.AttributeType),
                        new XAttribute("AttributeId", i.AttributeId),
                        new XAttribute("ColumnOrder", i.ColumnOrder.ToString())
                        )).ToList());

                    xmlElements = Convert.ToString(AttributexmlElements);
                }
                if (columnview != null)
                {
                    if (!string.IsNullOrEmpty(ViewName))
                    {
                        result = -1;

                    }
                    else
                    {

                        columnview.ModifyBy = UserId;
                        columnview.ModifyDate = DateTime.Now;
                        columnview.IsDefault = true;
                        if (Isgrid)
                            columnview.GridAttribute = xmlElements;
                        else
                            columnview.BudgetAttribute = xmlElements;
                        objDbMrpEntities.Entry(columnview).State = EntityState.Modified;
                        objDbMrpEntities.SaveChanges();

                        result = columnview.ViewId;
                    }


                }
                else
                {
                    columnview = new User_CoulmnView();
                    columnview.ViewName = ViewName;
                    columnview.CreatedBy = UserId;
                    columnview.CreatedDate = DateTime.Now;
                    columnview.IsDefault = true;
                    if (Isgrid)
                        columnview.GridAttribute = xmlElements;
                    else
                        columnview.BudgetAttribute = xmlElements;
                    objDbMrpEntities.Entry(columnview).State = EntityState.Added;
                    objDbMrpEntities.SaveChanges();
                    result = columnview.ViewId;
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return result;
        }

        #endregion

        public List<ColumnViewEntity> GetCutomefieldModel(int ClientId, bool IsGrid, out bool IsSelectall)
        {
            List<CustomAttribute> BasicFields = new List<CustomAttribute>();
            List<ColumnViewEntity> allattributeList = new List<ColumnViewEntity>();
            string attributexml = string.Empty;
            List<string> SelectedCustomfieldID = new List<string>();
            IsSelectall = false;
            try
            {

                var userview = objDbMrpEntities.User_CoulmnView.Where(a => a.CreatedBy == Sessions.User.ID).FirstOrDefault();
                if (userview == null)
                {
                    IsSelectall = true;
                }
                else
                {

                    if (IsGrid)
                        attributexml = Convert.ToString(userview.GridAttribute);

                    else
                        attributexml = Convert.ToString(userview.BudgetAttribute);

                    if (string.IsNullOrEmpty(attributexml))
                        IsSelectall = true;
                    if (attributexml != null)
                    {
                        var doc = XDocument.Parse(attributexml);
                        var items = (from r in doc.Root.Elements("attribute")
                                     select new
                                     {
                                         AttributeType = (string)r.Attribute("AttributeType"),
                                         AttributeId = (string)r.Attribute("AttributeId"),
                                         ColumnOrder = (string)r.Attribute("ColumnOrder")
                                     }).ToList();
                        SelectedCustomfieldID = items.Select(a => a.AttributeId).ToList();
                    }
                }
                if (IsGrid)
                {
                    DataTable dtColumnAttribute = GetCustomFieldList(ClientId);
                    if (dtColumnAttribute != null && dtColumnAttribute.Rows.Count > 0)
                    {

                        var columnattribute = dtColumnAttribute.AsEnumerable().Select(row => new CustomAttribute
                        {
                            EntityType = Convert.ToString(row["EntityType"]),
                            CustomFieldId = Convert.ToString(row["CustomFieldId"]),
                            CutomfieldName = Convert.ToString(row["Name"]),
                            ParentID = Convert.ToInt32(row["ParentId"])

                        }).ToList();

                        BasicFields = Enums.CommonGrid_Column.Select(row => new CustomAttribute
                        {
                            EntityType = "Common",
                            CustomFieldId = Convert.ToString(row.Key),
                            CutomfieldName = Convert.ToString(row.Value),
                            ParentID = 0
                        }).ToList();

                        List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                        string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();

                        var mqlfield = new CustomAttribute
                        {
                            EntityType = "Common",
                            CustomFieldId = "mql",
                            CutomfieldName = MQLTitle,
                            ParentID = 0
                        };
                        BasicFields.Add(mqlfield);
                        BasicFields.AddRange(columnattribute);
                    }



                    allattributeList = BasicFields.GroupBy(a => new { EntityType = a.EntityType }).Select(a => new ColumnViewEntity
                    {
                        EntityType = a.Key.EntityType,

                        AttributeList = BasicFields.Where(atr => atr.EntityType == a.Key.EntityType).Select(atr => new ColumnViewAttribute
                        {
                            CustomFieldId = atr.CustomFieldId,
                            CutomfieldName = atr.CutomfieldName,
                            ParentID = atr.ParentID,
                            IsChecked = SelectedCustomfieldID.Contains(atr.CustomFieldId) ? true : false
                        }).ToList()
                    }).ToList();
                }
                else
                {
                    allattributeList = Enum.GetNames(typeof(Enums.Budgetcolumn)).ToList().Select(row => new ColumnViewEntity
                  {
                      EntityType = row,
                      EntityIsChecked = SelectedCustomfieldID.Contains(row) ? true : false
                  }).ToList();
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return allattributeList;

        }

    }
}