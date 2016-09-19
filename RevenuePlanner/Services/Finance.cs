using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Finance : IFinance
    {
        private MRPEntities objDbMrpEntities;
        private CacheObject objCache = new CacheObject();
        public Finance()
        {
            objDbMrpEntities = new MRPEntities();
        }
        /// <summary>
        /// Add By Nishant Sheth
        /// Get List of custom columns list by client
        /// </summary>
        /// <returns></returns>
        public List<CustomColumnModel> GetCustomColumns()
        {
            List<CustomColumnModel> lstColumns = (from objColumnSet in objDbMrpEntities.Budget_ColumnSet
                                                  join objColumn in objDbMrpEntities.Budget_Columns on objColumnSet.Id equals objColumn.Column_SetId
                                                  join objCustomField in objDbMrpEntities.CustomFields on objColumn.CustomFieldId equals objCustomField.CustomFieldId
                                                  where objColumnSet.ClientId == Sessions.User.CID
                                                  && objColumn.IsTimeFrame == false
                                                  && objColumnSet.IsDeleted == false && objColumn.IsDeleted == false && objCustomField.IsDeleted == false
                                                  select new CustomColumnModel
                                                  {
                                                      ColName = objCustomField.Name,
                                                      CustomColumSetId = objColumnSet.Id,
                                                      CustomFieldId = objCustomField.CustomFieldId,
                                                      ValidationType = objColumn.ValidationType
                                                  }).ToList();
            return lstColumns;
        }
    }
}