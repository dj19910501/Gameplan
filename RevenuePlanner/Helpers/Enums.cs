using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

/*
 *  Author: Maninder Singh Wadhva
 *  Created Date: 11/28/2013
 *  Purpose: Holds enum for MRP.
 */

namespace RevenuePlanner.Helpers
{
    public class Enums
    {
        #region Plan
        /// <summary>
        /// Enum for tactic.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public enum TacticStatus
        {
            Created = 0,
            Submitted = 1,
            Decline = 2,
            Approved = 3,
            InProgress = 4,
            Complete = 5,
        }

        /// <summary>
        /// Data Dictionary to hold tactic status values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public static Dictionary<string, string> TacticStatusValues = new Dictionary<string, string>()
        {
            {TacticStatus.Created.ToString(), "Created"},
            {TacticStatus.Submitted.ToString(), "Submitted"},
            {TacticStatus.Decline.ToString(), "Declined"},
            {TacticStatus.Approved.ToString(), "Approved"},
            {TacticStatus.InProgress.ToString(), "In-Progress"},
            {TacticStatus.Complete.ToString(), "Complete"}
        };

        /// <summary>
        /// Data Dictionary to hold tactic status color code.
        /// Added By: Sohel Pathan
        /// Date: 12/05/2014
        /// </summary>
        public static Dictionary<string, string> TacticStatusColorCodes = new Dictionary<string, string>()
        {
            {"Created", "F6D87F"},
            {"Submitted", "EE8F7A"},
            {"Approved", "CC91BF"},
            {"Rejected", "D4D4D4"},
        };

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// Enum for plan status.
        /// </summary>
        public enum PlanStatus
        {
            Draft = 0,
            Published = 1,
        }

        /// <summary>
        /// Data Dictionary to hold plan status values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// </summary>
        public static Dictionary<string, string> PlanStatusValues = new Dictionary<string, string>()
        {
            {PlanStatus.Draft.ToString(), "Draft"},
            {PlanStatus.Published.ToString(), "Published"}
        };

        /// <summary>
        /// Enum for Stage Value.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 12/26/2013
        /// </summary>
        public enum InspectStage
        {
            INQ = 0,
            MQL = 1,
            CW = 2,
            Revenue = 3,
            ProjectedStageValue = 4,
            Cost = 5,
            TQL = 6,
            ADS = 7
        }

        /// <summary>
        /// Data Dictionary to hold Stage values.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 11/26/2013
        /// </summary>
        public static Dictionary<string, string> InspectStageValues = new Dictionary<string, string>()
        {
            {InspectStage.INQ.ToString(), "INQ"},
            {InspectStage.MQL.ToString(), "MQL"},
            {InspectStage.CW.ToString(), "CW"},
            {InspectStage.Revenue.ToString(), "Revenue"},
            {InspectStage.ProjectedStageValue.ToString(), "ProjectedStageValue"},
            {InspectStage.Cost.ToString(), "Cost"}
        };

        /// <summary>
        /// Enum for Yeat.
        /// Added By: Nirav Shah.
        /// Date: 1/7/2014
        /// </summary>
        public enum UpcomingActivities
        {
            thisyear = 0,
            thisquarter = 1,
            thismonth = 2,
            nextyear = 3,
            planYear = 4,
            previousyear = 5,
            lastyear = 6
        }
        /// <summary>
        /// Data Dictionary to hold Upcoming Activity values.
        /// Added By: Nirav shah.
        /// Date: 1/7/2014
        /// </summary>
        public static Dictionary<string, string> UpcomingActivitiesValues = new Dictionary<string, string>()
        {
            {UpcomingActivities.thisyear.ToString(), "this year"},
            {UpcomingActivities.thisquarter.ToString(),  "this quarter"},
            {UpcomingActivities.thismonth.ToString(), "this month"},
            {UpcomingActivities.nextyear.ToString(), "next year"},
            {UpcomingActivities.planYear.ToString(), "plan year"},
            {UpcomingActivities.previousyear.ToString(), "previous year"},
            {UpcomingActivities.lastyear.ToString(), "last year"}
        };
        #endregion

        #region Model
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// Enum for model status.
        /// </summary>
        public enum ModelStatus
        {
            Published = 0,
            Draft = 1
        }

        /// <summary>
        /// Data Dictionary to hold model status values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// </summary>
        public static Dictionary<string, string> ModelStatusValues = new Dictionary<string, string>()
        {
            {ModelStatus.Published.ToString(), "Published"},{ModelStatus.Draft.ToString(), "Draft"}
        };
        public enum FunnelFlag
        {
            M = 0,
            T = 1,
            S = 2

        }

        public static Dictionary<string, string> FunnelFlagValue = new Dictionary<string, string>()
        {
            {FunnelFlag.M.ToString(), "M"},
            {FunnelFlag.T.ToString(), "T"},
            {FunnelFlag.S.ToString(), "S"}
          
        };

        // Add By Nishant Sheth : 06-July-2015

        public enum PerPage
        {
            Five = 5,
            Ten = 10,
            Twenty = 20,
            TwentyFive = 25,
            Fifty = 50,
            Hundered = 100
        }
        public static Dictionary<string, string> PerPageSize = new Dictionary<string, string>()
        {
            {PerPage.Five.ToString(), "5"},
            {PerPage.Ten.ToString(), "10"},
            {PerPage.Twenty.ToString(), "20"},
            {PerPage.TwentyFive.ToString(), "25"},
            {PerPage.Fifty.ToString(), "50"},
            {PerPage.Hundered.ToString(), "100"}
          
        };

        public enum SortByRevenue
        {
            Revenue,
            Cost,
            ROI
        }
        public static Dictionary<string, string> SortByRevenueDrp = new Dictionary<string, string>()
        {
            {SortByRevenue.Revenue.ToString(), "revenueval"},
            {SortByRevenue.Cost.ToString(), "costval"},
            {SortByRevenue.ROI.ToString(), "roival"}
            
        };

        public enum SortByWaterFall
        {
            INQ,
            TQL,
            CW,
            ADS
        }
        public static Dictionary<string, string> SortByWaterFallDrp = new Dictionary<string, string>()
        {
            {SortByWaterFall.INQ.ToString(), "inqval"},
            {SortByWaterFall.TQL.ToString(), "tqlval"},
            {SortByWaterFall.CW.ToString(), "cwval"},
            {SortByWaterFall.ADS.ToString(), "adsval"}
            
        };
        // End By Nishant Sheth
        #endregion

        //#region User
        ///// <summary>
        ///// Enum for role.
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// </summary>
        //public enum Role
        //{
        //    SystemAdmin = 0,
        //    ClientAdmin = 1,
        //    Director = 2,
        //    Planner = 3,
        //}

        ///// <summary>
        ///// Data Dictionary to hold role code values.
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// </summary>
        //public static Dictionary<string, string> RoleCodeValues = new Dictionary<string, string>()
        //{
        //    {Role.SystemAdmin.ToString(), "SA"},
        //    {Role.ClientAdmin.ToString(), "CA"},
        //    {Role.Director.ToString(), "D"},
        //    {Role.Planner.ToString(), "P"}
        //};

        ///// <summary>
        ///// Enum for role codes.
        ///// Added By: Kuber B Joshi.
        ///// Date: 12/11/2013
        ///// </summary>
        //public enum RoleCodes
        //{
        //    SA,
        //    CA,
        //    D,
        //    P
        //}

        //#endregion

        #region Permission
        public enum Permission
        {
            None = 0,
            ViewOnly = 1,
            Full = 2
        }

        public enum ActiveMenu
        {
            Home = 0,
            Model = 1,
            Plan = 2,
            Boost = 3,
            Report = 4,
            Pref = 5,
            ExternalService,
            Organization,
            None = 999
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 06/18/2014
        /// Enum to identify application activity.
        /// Modified by sohel Pathan
        /// </summary>
        [System.Flags]
        public enum ApplicationActivity
        {
            UserAdministration = 1 << 0,
            UserAdmin = 1 << 1,
            IntegrationCredential = 1 << 2,
            IntegrationCredentialCreateEdit = 1 << 3,
            Model = 1 << 4,
            ModelCreateEdit = 1 << 5,
            Plan = 1 << 6,
            PlanCreate = 1 << 7,
            PlanEditSubordinates = 1 << 8,
            PlanEditAll = 1 << 9,
            TacticApproveForPeers = 1 << 10,
            TacticActualsAddEdit = 1 << 11,
            Boost = 1 << 12,
            BoostImprovementTacticCreateEdit = 1 << 13,
            BoostBestInClassNumberEdit = 1 << 14,
            Report = 1 << 15,
            ReportView = 1 << 16,
            Comments = 1 << 17,
            CommentsViewEdit = 1 << 18,
            TacticApproveOwn = 1 << 19
        }


        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 01/01/2014
        /// Enum for Active menu values.
        /// </summary>
        public static Dictionary<string, string> ActiveMenuValues = new Dictionary<string, string>()
        {
            {ActiveMenu.Home.ToString().ToLower(), "home"},
            {ActiveMenu.Model.ToString().ToLower(), "model"},
            {ActiveMenu.Plan.ToString().ToLower(), "plan"},
            {ActiveMenu.Boost.ToString().ToLower(), "boost"},
            {ActiveMenu.Report.ToString().ToLower(), "report"},
            {ActiveMenu.Pref.ToString().ToLower(), "user"},
            {ActiveMenu.ExternalService.ToString().ToLower(), "externalservice"},
            {ActiveMenu.Organization.ToString().ToLower(), "organization"}
        };

        #endregion

        /* Added by Mitesh Vaishnav for PL ticket #521 */
        public enum CustomRestrictionPermission
        {
            None = 0,
            ViewOnly = 1,
            ViewEdit = 2
        }
        public static Dictionary<string, string> CustomRestrictionValues = new Dictionary<string, string>(){
        { CustomRestrictionPermission.None.ToString(),"None"},
            { CustomRestrictionPermission.ViewOnly.ToString(),"View Only"},
            {CustomRestrictionPermission.ViewEdit.ToString(),"View/Edit"}
         };

        public enum UserActivityPermissionType
        {
            Yes,
            No
        }
        public enum UserPermissionMode
        {
            View,
            Edit,
            MyPermission
        }
        /* End Added by Mitesh Vaishnav for PL ticket #521 */
        /*Added by Mitesh Vaishnav for PL ticket #659*/
        public enum IntegrationInstanceType
        {
            Eloqua,
            Salesforce,
            Marketo,
            WorkFront
        }
        public static Dictionary<string, string> IntegrationActivity = new Dictionary<string, string>()
        {
            {"IntegrationInstanceId","Push Tactic Data"},
            {"IntegrationInstanceIdINQ","Pull Responses"},
            {"IntegrationInstanceIdMQL","Pull Qualified Leads"},
            {"IntegrationInstanceIdCW","Pull Closed Deals"},
            {"IntegrationInstanceIdProjMgmt","Sync Project Managment"} //added by Brad Gray for PL#1488
        };
        /*End :Added by Mitesh Vaishnav for PL ticket #659*/
        #region Notification
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// Enum for notification.
        /// </summary>
        public enum Notification
        {
            NewsAndUpdates = 0,
            WeeklyReport = 1,
            ReportIsShared = 2,
            PlanIsUpdated = 3,
            ModelIsUpdated = 4,
        }

        /// <summary>
        /// Added By: Kuber Joshi.
        /// Edited By : Kunal to add program and compaign related custom notifications.
        /// Date: 1/21/2014
        /// Enum for custom notification.
        /// </summary>
        public enum Custom_Notification
        {
            ShareTactic,
            UserCreated,
            TacticCommentAdded,
            TacticApproved,
            TacticDeclined,
            TacticSubmitted,
            ContactSupport,
            ChangePassword,
            ShareProgram,
            ShareCampaign,
            ProgramApproved,
            ProgramDeclined,
            ProgramSubmitted,
            CampaignApproved,
            CampaignDeclined,
            CampaignSubmitted,
            ProgramCommentAdded,
            CampaignCommentAdded,
            ShareReport,  //// Added By: Maninder Singh Wadhva for Share report functionality.
            ImprovementTacticCommentAdded, /// Added By Bhavesh Dobariya.
            ImprovementTacticApproved,
            ImprovementTacticDeclined,
            ImprovementTacticSubmitted,
            ShareImprovementTactic,
            ResetPasswordLink, // Added by : Dharmraj Mangukiya
            TacticOwnerChanged,  // Added by : Sohel Pathan on 14/11/2014 for PL ticket #708
            SyncIntegrationError,    //// Added by : Sohel Pathan on 02/01/2015 for PL ticket #1068
            ProgramOwnerChanged,  // Added by : Pratik on 03/03/2014 for PL ticket #711
            CampaignOwnerChanged  // Added by : Pratik on 03/03/2014 for PL ticket #711
        }
        #endregion

        #region Notification Type
        /// <summary>
        /// Added By: Kuber Joshi.
        /// Date: 12/31/2013
        /// Enum for notification type.
        /// </summary>
        public enum NotificationType
        {
            SM,
            AM,
            CM
        }
        #endregion


        #region Funnel Field
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 12/30/2013
        /// Enum for Funnel Field.
        /// </summary>
        public enum FunnelField
        {
            DatabaseSize,
            ConversionGate_SUS,
            OutboundGeneratedInquiries,
            InboundInquiries,
            TotalInquiries,
            ConversionGate_INQ,
            AQL,
            ConversionGate_AQL,
            TAL,
            ConversionGate_TAL,
            TQL,
            TGL,
            MQL,
            ConversionGate_TQL,
            SAL,
            SGL,
            ConversionGate_SAL,
            SQL,
            ConversionGate_SQL,
            ClosedWon,
            BlendedFunnelCR_Times,
            AverageDealsSize,
            ExpectedRevenue
        }
        #endregion

        #region Stage Type
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 12/30/2013
        /// Enum for Stage Type.
        /// </summary>
        public enum StageType
        {
            CR,
            SV,
            Size //added by uday for PL #501
        }
        #endregion

        #region Stage
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 12/30/2013
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Enum for Stage.
        /// </summary>
        public enum Stage
        {
            SUS,
            INQ,
            AQL,
            TAL,
            TQL,
            SAL,
            SQL,
            MQL,
            CW,
            ADS
        }
        #endregion

        #region Change log object
        public enum ChangeLog_TableName
        {
            Plan,
            Model
        }

        public enum ChangeLog_Actions
        {
            added,
            removed,
            updated,
            approved,
            declined,
            published,
            advanced,
            benchmarked,
            created,
            moved
        }

        public enum ChangeLog_ComponentType
        {
            [Description("plan")]
            plan,
            [Description("model")]
            model,
            [Description("campaign")]
            campaign,
            [Description("tactic type")]
            tactictype,
            [Description("program")]
            program,
            [Description("tactic")]
            tactic,
            [Description("tactic results")]
            tacticresults,
            [Description("")]
            empty,
            [Description("updated to")]
            updatedto,
            [Description("improvement tactic")]
            improvetactic,
            [Description("line item")]
            lineitem,

        }
        #endregion

        #region Plan Header

        public enum PlanHeader_Label
        {
            MQLLabel,
            ProjectedMQLLabel,
            Cost,
            Budget
        }

        public static Dictionary<string, string> PlanHeader_LabelValues = new Dictionary<string, string>()
        {
            {PlanHeader_Label.MQLLabel.ToString(), "MQL"},
            {PlanHeader_Label.ProjectedMQLLabel.ToString(), "Projected MQL"},
            {PlanHeader_Label.Cost.ToString(), "Cost"},
            {PlanHeader_Label.Budget.ToString(), "Budget"}
        };
        #endregion

        #region Quarter
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 1/6/2014
        /// Enum for Quarter.
        /// </summary>
        public enum Quarter
        {
            Q1,
            Q2,
            Q3,
            Q4,
            ALLQ
        }
        #endregion

        #region For XML Import

        #region Stage Velocity
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 01/17/2014
        /// Enum for Stage Velocity.
        /// </summary>
        public enum StageVelocity
        {
            OutboundSuspect,
            InboundSuspect,
            AQL,
            TAL,
            TQL,
            SAL,
            SQL,
            MQL
        }
        #endregion

        #endregion

        #region Sections
        /// <summary>
        /// Modified by: Mitesh vaishnav
        /// Add New section of LineItem.
        /// Enum for Sections.
        /// </summary>
        public enum Section
        {
            Campaign,
            Program,
            Tactic,
            ImprovementTactic,
            LineItem,
            Plan
        }
        #endregion

        #region Report
        /// <summary>
        /// Enum for report type.
        /// Added By: Maninder Singh Wadhva.
        /// </summary>
        public enum ReportType
        {
            Summary = 0,
            Revenue = 1,
            Conversion = 2,
            Budget = 3
        }

        /// <summary>
        /// Enum for Month.
        /// Added By: Bhavesh Dobariya.
        /// </summary>
        public enum ReportMonthDisplay
        {
            Jan = 0,
            Feb = 1,
            Mar = 2,
            April = 3,
            May = 4,
            Jun = 5,
            July = 6,
            Aug = 7,
            Sep = 8,
            Oct = 9,
            Nov = 10,
            Dec = 11
        }

        /// <summary>
        /// Data Dictionary to hold Month values.
        /// Added By: Bhavesh Dobariya.
        /// </summary>
        public static Dictionary<string, string> ReportMonthDisplayValues = new Dictionary<string, string>()
        {
            {ReportMonthDisplay.Jan.ToString(), "Jan"},
            {ReportMonthDisplay.Feb.ToString(), "Feb"},
            {ReportMonthDisplay.Mar.ToString(), "Mar"},
            {ReportMonthDisplay.April.ToString(), "Apr"},
            {ReportMonthDisplay.May.ToString(), "May"},
            {ReportMonthDisplay.Jun.ToString(), "Jun"},
            {ReportMonthDisplay.July.ToString(), "Jul"},
            {ReportMonthDisplay.Aug.ToString(), "Aug"},
            {ReportMonthDisplay.Sep.ToString(), "Sep"},
            {ReportMonthDisplay.Oct.ToString(), "Oct"},
            {ReportMonthDisplay.Nov.ToString(), "Nov"},
            {ReportMonthDisplay.Dec.ToString(), "Dec"}
        };

        #endregion

        #region Improvement

        /// <summary>
        /// Added By: Bhavesh Dobariya
        /// Date: 3/06/2014
        /// Enum for Metric Type.
        /// </summary>
        public enum MetricType
        {
            CR,
            SV,
            Size,
            Volume
        }

        #endregion

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:14/07/2014
        /// Enum for GoalType
        /// </summary>
        public enum PlanGoalType
        {
            INQ,
            MQL,
            Revenue
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:14/07/2014
        /// GoalType List based on Enum
        /// </summary>
        public static Dictionary<string, string> PlanGoalTypeList = new Dictionary<string, string>()
        {
            {PlanGoalType.INQ.ToString(), "inq"},
            {PlanGoalType.MQL.ToString(), "mql"},
            {PlanGoalType.Revenue.ToString(), "revenue"}
        };

        public enum DuplicationModule
        {
            Plan,
            Campaign,
            Program,
            Tactic,
            LineItem
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:14/07/2014
        /// Enum for AllocatedBy
        /// </summary>
        public enum PlanAllocatedBy
        {
            defaults,
            months,
            quarters,
            none
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:14/07/2014
        /// Allocated by List based on Enum
        /// </summary>
        public static Dictionary<string, string> PlanAllocatedByList = new Dictionary<string, string>()
        {
            {PlanAllocatedBy.defaults.ToString(), "default"},
            {PlanAllocatedBy.months.ToString(), "months"},
            {PlanAllocatedBy.quarters.ToString(), "quarters"},
            {PlanAllocatedBy.none.ToString(), "none"},
        };

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:05/08/2014
        /// Enum for Integration DataType Mapping
        /// </summary>
        public enum IntegrantionDataTypeMappingTableName
        {
            Plan_Campaign,
            Plan_Campaign_Program,
            Plan_Campaign_Program_Tactic,
            Plan_Improvement_Campaign,
            Plan_Improvement_Campaign_Program,
            Plan_Improvement_Campaign_Program_Tactic,
            Global  //  Added by Sohel Pathan on 02/12/2014 for PL ticket #993
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:05/08/2014
        /// Enum for Integration Types
        /// </summary>
        public enum IntegrationType
        {
            Eloqua,
            Salesforce,
            Marketo,
            WorkFront //added by Brad Gray for PL#1374
        }

        /// <summary>
        /// Added by: Dharmraj on 13-8-2014
        /// Enum for GameplanDatatypePull Type
        /// </summary>
        public enum GameplanDatatypePullType
        {
            INQ,
            MQL,
            CW
        }

        //Added By : Kalpesh Sharma PL #697 Default Line item type
        public enum LineItemTypes
        {
            None
        }

        /// <summary>
        /// Enum for custom field type
        /// Added By : Mitesh Vaishnav for PL ticket #718
        /// </summary>
        public enum CustomFieldType
        {
            TextBox,
            DropDownList
        }

        /// <summary>
        /// Enum for custom field entity type
        /// Added by Mitesh Vaishnav for PL ticket #718
        /// </summary>
        public enum EntityType
        {
            //Modified for #1282
            Campaign,
            Program,
            Tactic,
            Plan,
            ImprovementTactic
        }

        #region "Inspect Popup"


        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 28/10/2014
        /// Description : Enum for Inspect Popup Mode
        /// </summary>
        public enum InspectPopupMode
        {
            Add,
            Edit,
            ReadOnly
        }

        public enum InspectPopupRequestedModules
        {
            Index,
            Assortment,
            ApplyToCalendar,
            Budgeting
        }


        /// <summary>
        /// Added By : Pratik
        /// Added Date : 05/11/2014
        /// Description : Enum for Resubmission Popup open from
        /// </summary>
        public enum ResubmissionOpenFrom
        {
            Tactic,
            ImprovementTactic
        }

        /// <summary>
        /// Added By : Viral Kadiya
        /// Added Date : 17/11/2014
        /// Description : Enum for Inspect Popup Tabs
        /// </summary>
        public enum InspectPopupTabs
        {
            Setup,
            Review,
            Actuals,
            Budget
        }

        /// <summary>
        /// Added By : Viral Kadiya
        /// Added Date : 17/11/2014
        /// Description : Enum for Activity Type
        /// </summary>
        public enum PlanEntity
        {
            Plan,
            Campaign,
            Program,
            Tactic,
            LineItem,
            ImprovementTactic
        }

        public static Dictionary<string, string> PlanEntityValues = new Dictionary<string, string>()
        {
            {PlanEntity.Plan.ToString(), "Plan"},
            {PlanEntity.Campaign.ToString(), "Campaign"},
            {PlanEntity.Program.ToString(), "Program"},
            {PlanEntity.Tactic.ToString(), "Tactic"},
            {PlanEntity.LineItem.ToString(), "LineItem"},
            {PlanEntity.ImprovementTactic.ToString(), "Improvement Tactic"}
        };

        #endregion

        /// <summary>
        /// Pull responses MQL for Eloqua, Actual field names
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>22/12/2014</CreatedDate>
        public enum PullResponsesMQLFields
        {
            MQLDate,
            CampaignId,
            ViewId,
            ListId
        }

        /// <summary>
        /// PermissionCode MQL for Client_Integration_Permission table
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>09/01/2015</CreatedDate>
        public enum ClientIntegrationPermissionCode
        {
            MQL
        }

        /// <summary>
        /// 12 months enum for get full name of month in budgeting
        /// <CreatedBy>Mitesh Vaishnav</CreatedBy>
        /// <CreatedDate>10/03/2015</CreatedDate>
        /// </summary>
        public enum Months
        {
            January,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
        public static Dictionary<int, string> YearMonths = new Dictionary<int, string>()
        {
            {1,"January"},
            {2,"February"},
            {3,"March"},
            {4,"April"},
            {5,"May"},
            {6,"June"},
            {7,"July"},
            {8,"August"},
            {9,"September"},
            {10,"October"},
            {11,"November"},
            {12,"December"}
        };
        public enum QuarterWithSpace
        {

            Quarter1,
            Quarter2,
            Quarter3,
            Quarter4,
        }
        public static Dictionary<string, string> Quarters = new Dictionary<string, string>()
       {
           {Enums.QuarterWithSpace.Quarter1.ToString(),"Quarter 1"},
           {Enums.QuarterWithSpace.Quarter2.ToString(),"Quarter 2"},
           {Enums.QuarterWithSpace.Quarter3.ToString(),"Quarter 3"},
           {Enums.QuarterWithSpace.Quarter4.ToString(),"Quarter 4"}
       };
        public static Dictionary<int, string> YearQuarters = new Dictionary<int, string>()
       {
           {1,"Quarter 1"},
           {4,"Quarter 2"},
           {7,"Quarter 3"},
           {10,"Quarter 4"}
       };
        public enum BudgetTab
        {
            Allocated = 0,
            Planned = 1,
            Actual = 2
        }

        /// <summary>
        /// Get ViewBya
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>05/03/2015</CreatedDate>
        public enum ViewBy
        {
            Campaign = 0
        }
        /// <summary>
        /// ViewBy Frequencies
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>05/03/2015</CreatedDate>
        public enum ViewByAllocated
        {
            Monthly,
            Quarterly
        }

        /// <summary>
        /// TOP Revenue Type
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>23/03/2015</CreatedDate>
        public enum TOPRevenueType
        {
            Revenue,
            Performance,
            Cost,
            ROI
        }
    }

    #region Authorization
    public static class ActionItem
    {
        public static string Home = "HOME";
        public static string Model = "MODEL";
        public static string Plan = "PLAN";
        public static string Boost = "BOOST";
        public static string Report = "REPORT";
        public static string Pref = "PREF";
    }
    #endregion

    #region "Gant Chart"
    public enum GanttTabs
    {
        Tactic = 0,
        Request = 1,
        None = 2
    }


    public enum PlanGanttTypes
    {
        Tactic,
        Request,
        Custom,
        Stage,
        Status
    }

    #endregion

    /// <summary>
    /// Added By : Kalpesh Sharma PL#582
    /// </summary>
    public static class SyncFrequencys
    {
        public static string Hourly = "Hourly";
        public static string Daily = "Daily";
        public static string Weekly = "Weekly";
        public static string Monthly = "Monthly";
        public static string PM = "PM";
    }

    public static class ActivityType
    {
        public const string ActivityMain = "main";
        public const string ActivityPlan = "plan";
        public const string ActivityCampaign = "campaign";
        public const string ActivityProgram = "program";
        public const string ActivityTactic = "tactic";
        public const string ActivityLineItem = "lineitem";
        public const string ActivityCustomField = "customfield";
    }

    public static class ReportColumnType
    {
        public const string Planned = "Planned";
        public const string Actual = "Actual";
        public const string Allocated = "Allocated";
    }

    public static class ReportTabType
    {
        public const string Plan = "Plan";
    }

    public static class ReportTabTypeText
    {
        public const string Plan = "Plan";
    }

}