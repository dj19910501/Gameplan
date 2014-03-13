using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

/*
    Author: Kuber Joshi
    Created Date: 02/10/2014
    Purpose: Plan Improvement
 */

namespace RevenuePlanner.Controllers
{
    public class ImprovementController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private const int DEF_COST_VALUE = 0;

        #endregion

       
    }
}
