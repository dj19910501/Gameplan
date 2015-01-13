using System;

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
            Eloqua
        }

        public enum PullCWActualField
        {
            Stage,
            Timestamp,
            CampaignID,
            Amount
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
            ImportActual
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
            Tactic
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
        #region Custom Naming Structure

        /// <summary>
        /// Enum for table's name of custom namimg structure
        /// Added by Mitesh Vaishnav for PL ticket #1000
        /// </summary>
        public enum CustomNamingTables
        {
            Audience,
            BusinessUnit,
            CustomField,
            Geography,
            Plan_Campaign,
            Plan_Campaign_Program,
            Plan_Campaign_Program_Tactic,
            Vertical,
            TacticType
        }

        public enum clientAcivity
        {
            CustomCampaignNameConvention
        }
        #endregion
    }
}
