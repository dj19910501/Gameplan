using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RevenuePlanner.Models
{
    public class HomePlanModel
    {
        public HomePlanModelHeader objplanhomemodelheader { get; set; }
        public HomePlan objHomePlan { get; set; }

        //Start Maninder Singh Wadhva : 11/26/2013 - planId.
        public int PlanId;
        //End Maninder Singh Wadhva : 11/26/2013 - planId.

        //Start Maninder Singh Wadhva : 11/27/2013 - Director.
        //    public bool IsDirector;
        //End Maninder Singh Wadhva : 11/27/2013 - Director.

        //Start Maninder Singh Wadhva : 11/25/2013 - List of geographies and user.
        public List<Geography> objGeography { get; set; }
        public List<BDSService.User> objIndividuals { get; set; }
        //End Maninder Singh Wadhva : 11/25/2013 - List of geographies and user.

        //  public List<SelectListItem> plans { get; set; }

        //Start Maninder Singh Wadhva : 12/03/2013 - plan title.
        public string PlanTitle;
        //End Maninder Singh Wadhva : 12/03/2013 - plan title.

        public List<string> CollaboratorId;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:g}")]
        public DateTime LastUpdatedDate { get; set; }
        public List<SelectListItem> BusinessUnitIds { get; set; }
    }
    public class HomePlan
    {
        public bool IsDirector { get; set; }
        public List<SelectListItem> plans { get; set; }
    }
    public class HomePlanModelHeader
    {
        public double MQLs { get; set; }
        public double Budget { get; set; }
        public int TacticCount { get; set; }
        public string mqlLabel { get; set; }
        public string costLabel { get; set; }
        // public List<string> UpcomingActivity { get; set; }
        public List<SelectListItem> UpcomingActivity { get; set; }
    }

    public class InspectModel
    {
        public int PlanTacticId { get; set; }

        public string TacticTitle { get; set; }

        public string TacticTypeTitle { get; set; }

        public string CampaignTitle { get; set; }

        public string ProgramTitle { get; set; }

        public string Status { get; set; }

        public int TacticTypeId { get; set; }

        public int VerticalId { get; set; }

        public string ColorCode { get; set; }

        public string Description { get; set; }

        public int AudienceId { get; set; }

        public int PlanCampaignId { get; set; }

        public int PlanProgramId { get; set; }

        public string Owner { get; set; }

        public Guid OwnerId { get; set; }

        public Guid BusinessUnitId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public long? INQs { get; set; }

        public long? INQsActual { get; set; }

        public double? MQLs { get; set; }

        public double? MQLsActual { get; set; }

        public string VerticalTitle { get; set; }

        public string AudiencTitle { get; set; }

        public double? CWs { get; set; }

        public double? CWsActual { get; set; }

        public double? Revenues { get; set; }

        public double? RevenuesActual { get; set; }

        public double? Cost { get; set; }

        public double? CostActual { get; set; }

        public double? ROI { get; set; }

        public double? ROIActual { get; set; }
    }

    public class InspectReviewModel
    {
        public int PlanTacticId { get; set; }

        public int PlanCampaignId { get; set; } //PlanCampaignId property added for InspectReview section of Campaign Inspect Popup

        public int PlanProgramId { get; set; } //PlanProgramId property added for InspectReview section of Program Inspect Popup

        public string Comment { get; set; }

        public DateTime CommentDate { get; set; }

        public string CommentedBy { get; set; }

        public Guid CreatedBy { get; set; }

    }

    public class InspectActual
    {
        public int PlanTacticId { get; set; }

        public string StageTitle { get; set; }

        public string Period { get; set; }

        public int ActualValue { get; set; }

        public int TotalINQActual { get; set; }

        public int TotalMQLActual { get; set; }

        public int TotalCWActual { get; set; }

        public double TotalRevenueActual { get; set; }

        public double TotalCostActual { get; set; }

        public double ROI { get; set; }

        public double ROIActual { get; set; }

		public bool IsActual { get; set; }
		
    }

    public class ActivityChart
    {
        public string NoOfActivity { get; set; }
        public string Month { get; set; }
        public string Color { get; set; }
    }
}