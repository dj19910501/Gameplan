﻿using System.Collections.Generic;
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
            Revenue = 3
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
            {InspectStage.Revenue.ToString(), "Revenue"}
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
            previousyear = 5
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
            {UpcomingActivities.previousyear.ToString(), "previous year"}
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

        #endregion

        #region User
        /// <summary>
        /// Enum for role.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public enum Role
        {
            SystemAdmin = 0,
            ClientAdmin = 1,
            Director = 2,
            Planner = 3,
        }

        /// <summary>
        /// Data Dictionary to hold role code values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public static Dictionary<string, string> RoleCodeValues = new Dictionary<string, string>()
        {
            {Role.SystemAdmin.ToString(), "SA"},
            {Role.ClientAdmin.ToString(), "CA"},
            {Role.Director.ToString(), "D"},
            {Role.Planner.ToString(), "P"}
        };

        /// <summary>
        /// Enum for role codes.
        /// Added By: Kuber B Joshi.
        /// Date: 12/11/2013
        /// </summary>
        public enum RoleCodes
        {
            SA,
            CA,
            D,
            P
        }

        #endregion

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
            None = 999
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
            {ActiveMenu.ExternalService.ToString().ToLower(), "externalservice"}
        };

        #endregion

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
            ResetPasswordLink // Added by : Dharmraj Mangukiya
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

        #region Funnel
        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 12/30/2013
        /// Enum for Funnel.
        /// </summary>
        public enum Funnel
        {
            Marketing,
            Teleprospecting,
            Sales
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
            SV
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
            CW
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
            created
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
        /// Added By: Kunal Shrimali
        /// Enum for Sections.
        /// </summary>
        public enum Section
        {
            Campaign,
            Program,
            Tactic,
            ImprovementTactic
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
            Conversion = 2
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
        Vertical = 1,
        Stage = 2,
        Audience = 3,
        BusinessUnit = 4,
        Request = 5
    }
    #endregion
}