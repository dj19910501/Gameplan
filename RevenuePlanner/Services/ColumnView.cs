using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

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
        public DataTable GetCustomFieldList(Guid ClientId)
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
        public int SaveColumnView(Guid UserId, string ViewName)
        {
            int result = 0;
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


                if (columnview != null)
                {
                    if (!string.IsNullOrEmpty(ViewName))
                    {
                        result = -1;

                    }
                    else
                    {
                        result = columnview.ViewId;
                    }
                     

                }
                else
                {
                        User_CoulmnView objcolumnview = new User_CoulmnView();
                        objcolumnview.ViewName = ViewName;
                        objcolumnview.CreatedBy = UserId;
                        objcolumnview.CreatedDate = DateTime.Now;
                        objcolumnview.IsDefault = false;
                        objDbMrpEntities.Entry(objcolumnview).State = EntityState.Added;
                        objDbMrpEntities.SaveChanges();
                        result = objcolumnview.ViewId;
                }
               // }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return result;
        }

        #endregion
        #region method to save ColumnView attribute
        public int SaveColumnViewAttribute(int ViewId, List<AttributeDetail> lstAttributeDetail)
        {
            int result = 0;
            User_CoulmnView_attribute objColumnattribute;
            List<User_CoulmnView_attribute> ListobjColumnattribute;
            try
            {
                if (lstAttributeDetail != null)
                {
                    ListobjColumnattribute = objDbMrpEntities.User_CoulmnView_attribute.Where(view => view.ViewId == ViewId).ToList();
                    if(ListobjColumnattribute != null && ListobjColumnattribute.Count > 0)
                    {
                        ListobjColumnattribute.ForEach(x => objDbMrpEntities.Entry(x).State = EntityState.Deleted);
                    }
                    foreach(AttributeDetail objattribute in lstAttributeDetail)
                    {
                        objColumnattribute = new User_CoulmnView_attribute();
                        objColumnattribute.ViewId = ViewId;
                        objColumnattribute.AttributeId = objattribute.AttributeId;
                        objColumnattribute.AttributeType = objattribute.AttributeType;
                        objColumnattribute.ColumnOrder = objattribute.ColumnOrder;
                        objDbMrpEntities.Entry(objColumnattribute).State = EntityState.Added;
                    }
                    result=objDbMrpEntities.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return result;
        }

        #endregion
    }
}