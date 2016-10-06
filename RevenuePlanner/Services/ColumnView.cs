using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
            DataTable CustomFieldList = new DataTable();
            ///If connection is closed then it will be open
            SqlConnection Connection = objDbMrpEntities.Database.Connection as SqlConnection;
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
                    adp.Fill(CustomFieldList);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
            return CustomFieldList;
        }
        #endregion
        #region method to save ColumnView
        public int SaveColumnView(int UserId, string ViewName, List<AttributeDetail> AttributeDetail, bool Isgrid = true)
        {
            int result = 0;
            string xmlElements = string.Empty;
          
                User_CoulmnView columnview = new User_CoulmnView();
                //checks if columnview with the same view name already exists.
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
                    XElement AttributexmlElements = new XElement("ViewDetail", AttributeDetail.Select(i => new XElement("attribute",
                        new XAttribute("AttributeType", i.AttributeType),
                        new XAttribute("AttributeId", i.AttributeId),
                        new XAttribute("ColumnOrder", Convert.ToString(i.ColumnOrder))
                        )).ToList());

                    xmlElements = Convert.ToString(AttributexmlElements);

                    if (columnview != null)
                    {
                        if (!string.IsNullOrEmpty(ViewName))
                        {
                            result = -1;

                        }
                        else
                        {
                            result = UpdateColumnView(columnview, Isgrid, UserId, xmlElements);
                        }
                    }
                    else
                    {
                        result = AddNewColumnView(ViewName, Isgrid, UserId, xmlElements);
                    }
                }
            
            return result;
        }

        #endregion

        public List<ColumnViewEntity> GetCustomfieldModel(int ClientId, bool IsGrid, out bool IsSelectall, int UserId)
        {
            List<CustomAttribute> BasicFields = new List<CustomAttribute>();
            List<ColumnViewEntity> allattributeList = new List<ColumnViewEntity>();
            string attributexml = string.Empty;
            List<string> SelectedCustomfieldID = new List<string>();
            IsSelectall = false; //to check if no particular view exist for user then select all the columns by default.
           
                User_CoulmnView userview = objDbMrpEntities.User_CoulmnView.Where(a => a.CreatedBy == UserId).FirstOrDefault();
                if (userview == null)
                {
                    IsSelectall = true;
                }
                else
                {
                    if (IsGrid)
                    {
                        attributexml = Convert.ToString(userview.GridAttribute);
                    }
                    else
                    {
                        attributexml = Convert.ToString(userview.BudgetAttribute);
                    }
                    if (string.IsNullOrEmpty(attributexml))
                    {
                        IsSelectall = true;
                    }
                    else
                    {
                        //Getting xml data to list
                        XDocument doc = XDocument.Parse(attributexml);
                        List<AttributeDetail> items = (from r in doc.Root.Elements("attribute")
                                                       select new AttributeDetail
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
                        //adding custom fields of all entities.
                        List<CustomAttribute> columnattribute = dtColumnAttribute.AsEnumerable().Select(row => new CustomAttribute
                        {
                            EntityType = Convert.ToString(row["EntityType"]).Trim(),
                            CustomFieldId = Convert.ToString(row["CustomFieldId"]),
                            CutomfieldName = Convert.ToString(row["Name"]).Trim(),
                            ParentID = Convert.ToInt32(row["ParentId"])

                        }).ToList();

                        //adding the basic fields of the grid
                        BasicFields = Enums.CommonGrid_Column.Select(row => new CustomAttribute
                        {
                            EntityType = "Common",
                            CustomFieldId = Convert.ToString(row.Key),
                            CutomfieldName = Convert.ToString(row.Value),
                            ParentID = 0
                        }).ToList();

                        //adding the mql column of grid separately as the mql title changes as per client configuration. 
                        List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                        string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();
                        CustomAttribute mqlfield = new CustomAttribute
                        {
                            EntityType = "Common",
                            CustomFieldId = Convert.ToString(Enums.PlanGoalType.MQL),
                            CutomfieldName = MQLTitle,
                            ParentID = 0
                        };
                        BasicFields.Add(mqlfield);

                        CustomAttribute revenuefield = new CustomAttribute
                         {
                             EntityType = "Common",
                             CustomFieldId = Convert.ToString(Enums.PlanGoalType.Revenue).ToLower(),
                             CutomfieldName = Convert.ToString(Enums.PlanGoalType.Revenue),
                             ParentID = 0
                         };
                        BasicFields.Add(revenuefield);

                        BasicFields.AddRange(columnattribute);
                    }

                    //adding all attributes to single list.
                    allattributeList = BasicFields.GroupBy(a => a.EntityType, StringComparer.OrdinalIgnoreCase).Select(a => new ColumnViewEntity
                    {
                        EntityType = new CultureInfo("en-US").TextInfo.ToTitleCase(a.Key),
                        AttributeList = BasicFields.Where(atr => atr.EntityType.ToLower() == a.Key.ToLower()).Select(atr => new ColumnViewAttribute
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
                    if (IsSelectall)
                    {
                        SelectedCustomfieldID.Add(Convert.ToString(Enums.Budgetcolumn.Planned));
                        IsSelectall = false;
                    }
                    allattributeList = Enum.GetNames(typeof(Enums.Budgetcolumn)).ToList().Select(row => new ColumnViewEntity
                    {
                        EntityType = row,
                        EntityIsChecked = SelectedCustomfieldID.Contains(row) ? true : false
                    }).ToList();
                }
          
            return allattributeList;
        }

        public int UpdateColumnView(User_CoulmnView columnview, bool Isgrid, int UserId, string xmlElements)
        {
            int result = 0;
            
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
            
            return result;
        }

        public int AddNewColumnView(string ViewName, bool Isgrid, int UserId, string xmlElements)
        {
            User_CoulmnView columnview = new User_CoulmnView();
            int result = 0;
          
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
           
            return result;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Pass the xmldocument of user saved column view xml value and it's return the attribute value
        /// </summary>
        public List<AttributeDetail> UserSavedColumnAttribute(XDocument doc)
        {
            List<AttributeDetail> items = new List<AttributeDetail>();
           
                int colOrder = 0;
                items = (from r in doc.Root.Elements("attribute")
                         select new AttributeDetail
                         {
                             AttributeType = (string)r.Attribute("AttributeType"),
                             AttributeId = (string)r.Attribute("AttributeId"),
                             ColumnOrder = (string)r.Attribute("ColumnOrder")
                         }).OrderBy(col => int.TryParse(col.ColumnOrder, out colOrder))
                           .ToList();
          
            return items;
        }

        public List<CustomFieldOptionModel> GetCustomFiledOptionList(int clientID)
        {
            List<CustomFieldOptionModel> optionlist = new List<CustomFieldOptionModel>();
           
                DataTable dtColumnAttribute = GetCustomFieldList(clientID);

                if (dtColumnAttribute != null && dtColumnAttribute.Rows.Count > 0)
                {
                    List<CustomAttribute> columnattribute = dtColumnAttribute.AsEnumerable().Select(row => new CustomAttribute
                    {
                        EntityType = Convert.ToString(row["EntityType"]),
                        CustomFieldId = Convert.ToString(row["CustomFieldId"]),
                        CutomfieldName = Convert.ToString(row["Name"]),
                        ParentID = Convert.ToInt32(row["ParentId"]),
                        CustomfiledType = Convert.ToString(row["CustomFieldType"])

                    }).ToList();
                    List<CustomFieldDependency> DependencyList = objDbMrpEntities.CustomFieldDependencies.Where(a => a.IsDeleted == false).Select(a => a).ToList();

                    List<int> customfieldid = columnattribute.Select(a => Convert.ToInt32(a.CustomFieldId)).ToList();
                    List<CustomFieldOption> optlist = objDbMrpEntities.CustomFieldOptions.Where(a => a.CustomField.ClientId == clientID && a.IsDeleted == false && customfieldid.Contains(a.CustomFieldId)).ToList();

                    optionlist = optlist.Select(o => new CustomFieldOptionModel
                        {
                            ChildOptionId = DependencyList.Select(list => list.ChildOptionId).ToList().Contains(o.CustomFieldOptionId) ? true : false,
                            ParentOptionId = DependencyList.Where(b => b.ChildOptionId == o.CustomFieldOptionId).Select(b => b.ParentOptionId).ToList(),
                            customFieldOptionId = o.CustomFieldOptionId,
                            ChildOptionIds = DependencyList.Where(Child => Child.ParentOptionId == o.CustomFieldOptionId).Select(list => list.ChildOptionId).ToList(),
                            value = o.Value,
                            customFieldId = o.CustomFieldId
                        }).OrderBy(o => o.value).ToList();
                }
           
            return optionlist;
        }
       
    }
}