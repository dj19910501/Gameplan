using System;
using System.Collections.Generic;

namespace Integration.Helper
{
    public class Enums
    {
        public enum Mode
        {
            Create,
            Update,
            Delete,
            None
        }
        public enum IntegrationType
        {
            Salesforce,
            Eloqua,
            WorkFront,
            Marketo
        }

        public enum WorkFrontTacticApprovalObject
        {
            Project,
            Request,
            Project2 // Added by Arpita Soni for Ticket #2279 on 06/21/2016
        }
        // Add By Nishant Sheth
        public enum IntegrationTypeAttribute
        {
            Host
        }

        // Add By Nishant Sheth
        // enums for integration attributes
        public enum EntityIntegrationAttribute
        {
            MarketoUrl,
            EloquaBaseUrl
        }

        public enum PullCWActualField
        {
            Stage,
            Timestamp,
            CampaignID,
            Amount,
            ResponseDate,
            LastModifiedDate
        }

        public enum PullResponseActualField
        {
            Status,
            Timestamp,
            CampaignID
        }

        public enum IntegrationInstanceSectionName
        {
            PushTacticData,
            PullResponses,
            PullQualifiedLeads,
            PullClosedDeals,
            ImportActual,
            PullMQL
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:03/12/2014
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
            Global
        }

        /// <summary>
        /// Added by: Sohel Pathan
        /// Date:03/12/2014
        /// Enum for Entity Type For Custom Fields
        /// </summary>
        public enum EntityType
        {
            Campaign,
            Program,
            Tactic,
            ImprovementCampaign,
            ImprovementProgram,
            ImprovementTactic,
            IntegrationInstance //Added by Brad Gray 07/28/2015 for logging sync errors at Instance level
        }

        /// <summary>
        /// Added By: Sohel Pathan
        /// Date: 04/12/2014
        /// Enum for custom field type
        /// </summary>
        public enum CustomFieldType
        {
            TextBox,
            DropDownList
        }

        /// <summary>
        /// Added By: Pratik
        /// Date: 30/12/2014
        /// Enum for custom field MQL
        /// </summary>
        public enum CustomeFieldNameMQL
        {
            MQLDate,
            CampaignId,
            ViewId,
            ListId
        }

        #region Custom Naming Structure

        /// <summary>
        /// Enum for table's name of custom namimg structure
        /// Added by Mitesh Vaishnav for PL ticket #1000
        /// </summary>
        public enum CustomNamingTables
        {
            CustomField,
            Plan_Campaign,
            Plan_Campaign_Program,
            Plan_Campaign_Program_Tactic,
            TacticType
        }

        public enum clientAcivity
        {
            CustomCampaignNameConvention
        }
        #endregion

        /// <summary>
        /// Added By: Sohel Pathan
        /// Date: 02/01/2015
        /// Enum for custom notification.
        /// </summary>
        public enum Custom_Notification
        {
            SyncIntegrationError
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
        /// sync status enum for error email
        /// </summary>
        public enum SyncStatus
        {
            Header,
            Success,
            Error,
            Warning,
            Info,
            InProgress
        }

        public static Dictionary<string, string> SyncStatusValues = new Dictionary<string, string>()
        {
             {SyncStatus.InProgress.ToString(), "In-Progress"}
        };

        /// <summary>
        /// IntegrationInstance Message Operation
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>21/07/2015</CreatedDate>
        public enum MessageOperation
        {
            Start,
            End,
            //Create,
            None
        }

        /// <summary>
        /// IntegrationInstance Message Label
        /// </summary>
        /// <CreatedBy>Viral Kadiya</CreatedBy>
        /// <CreatedDate>21/07/2015</CreatedDate>
        public enum MessageLabel
        {
            Error,
            Success,
            Info,
            None
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Date: 07/22/2015
        /// Enum for plan status.
        /// </summary>
        public enum PlanStatus
        {
            Draft = 0,
            Published = 1,
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Date: 07/22/2015
        /// Enum for plan status.
        /// </summary>
        public static Dictionary<string, string> PlanStatusValues = new Dictionary<string, string>()
        {
            {PlanStatus.Draft.ToString(), "Draft"},
            {PlanStatus.Published.ToString(), "Published"}
        };	
      public enum ActualFields
        {
            ActivityType,
            Cost,
            CostActual,
            CreatedBy,
            Description,
            EffectiveDate,
            EndDate,
            StartDate,
            Status,
            TacticType,
            Title,
            PlanName,
            Other
        }
        public static Dictionary<string, string> ActualFieldDatatype = new Dictionary<string, string>()
        {
            {Enums.ActualFields.ActivityType.ToString(),"string,text"},
            {Enums.ActualFields.Cost.ToString(),"int,double,float,numeric"},
            {Enums.ActualFields.CostActual.ToString(),"int,double,float,numeric"},
            {Enums.ActualFields.CreatedBy.ToString(),"string,text"},
            {Enums.ActualFields.Description.ToString(),"string,text"},
            {Enums.ActualFields.EffectiveDate.ToString(),"date,datetime"},
            {Enums.ActualFields.EndDate.ToString(),"date,datetime"},
            {Enums.ActualFields.StartDate.ToString(),"date,datetime"},
            {Enums.ActualFields.Status.ToString(),"string,text"},
            {Enums.ActualFields.TacticType.ToString(),"string,text"},
            {Enums.ActualFields.Title.ToString(),"string,text"},
            {Enums.ActualFields.PlanName.ToString(),"string,text"},
            {Enums.ActualFields.Other.ToString(),"string,text"}
};
        public enum SFDCGlobalFields
        {
            PlanName
        }

        public enum ApiIntegrationData
        {
            CampaignFolderList,
            Programtype
        }
        public enum MarketoAPIEventNames
        {
            APIcall,
            Authentication,
            FieldMapping,
            DataMapping,
            SystemError,
            NoRecord,
            GetProgramData,
            Token,
            InvalidConnection,
            ErrorInSettingDestinationConnectin,
            RequiredTagNotExist,
            FetchUserInfo,
            Parameter,
            InsertProgram,
            UpdateProgram
        }
    }
}
