using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services
{
    public interface IColumnView
    {
        int SaveColumnView(int UserId, string ViewName, List<AttributeDetail> AttributeDetail, bool Isgrid = true);
        DataTable GetCustomFieldList(int ClientId);
        List<ColumnViewEntity> GetCustomfieldModel(int ClientId, bool Isgrid, out bool IsSelectall, int UserId);
    }
}