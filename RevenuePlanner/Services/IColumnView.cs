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
        int SaveColumnView(Guid UserId,string ViewName);
        int SaveColumnViewAttribute(int ViewId, List<AttributeDetail> AttributeDetail);
        DataTable GetCustomFieldList(Guid ClientId);
    }
}