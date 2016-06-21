
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using Integration.Helper;
using System.Collections.Generic;
using System.Linq;
using RevenuePlanner.Models;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using System.ComponentModel;


namespace Integration.WorkFront
{
    /// <summary>
    /// Creates a type-safe representation of the Fieldss used in the Stream API
    /// Each object in WorkFront is given a unique URI consisting of the object type and 
    /// the URI. Example: '/attask/api/proj/4c78821c0000d6fa8d5e52f07a1d54d0'
    /// More information: https://developers.attask.com/api-docs/api-explorer/
    /// </summary>
    public class Fields
    {

        /// Very good method to Override ToString on Enums
        /// Case : Suppose your enum value is EncryptionProviderType and you want 
        /// enumVar.Tostring() to retrun "Encryption Provider Type" then you should use this method.
        /// Prerequisite : All enum members should be applied with attribute [Description("String to be returned by Tostring()")]
        /// Example : 
        ///  enum ExampleEnum
        ///  {
        ///   [Description("One is one")]
        ///    ValueOne = 1,
        ///    [Description("Two is two")]
        ///    ValueTow = 2
        ///  }
        ///  
        ///  in your class
        ///  ExampleEnum enumVar = ExampleEnum.ValueOne ;
        ///  Console.WriteLine(ToStringEnums(enumVar));
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string ToStringEnums(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public enum GamePlanFields
        {
            [Description("Title")]
            TITLE = 1,
            [Description("Description")]
            DESCRIPTION = 3,
            [Description("StartDate")]
            START_DATE = 5,
            [Description("EndDate")]
            END_DATE = 7,
            [Description("Cost")]
            COST = 10,
            [Description("TacticBudget")]
            TACTIC_BUDGET = 12,
            [Description("Status")]
            STATUS = 14,
            [Description("CreatedBy")]
            CREATEDBY = 16

        }

        public static List<string> ReturnAllWorkFrontFields_AsUserText()
        {
            List<string> projFields = new List<string>();
            projFields.Add(WorkFrontField.NAME.ToString());
            projFields.Add(WorkFrontField.DESCRIPTION.ToString());
            projFields.Add(WorkFrontField.WORKFRONTPROJECTSTATUS.ToString());
            projFields.Add(WorkFrontField.PROJECTEDCOMPLETIONDATE.ToString());
            projFields.Add(WorkFrontField.BUDGET.ToString());
            return projFields;
        }

        public static List<string> ReturnAllWorkFrontFields_AsAPI()
        {
            List<string> pfields = new List<string>();
            pfields.Add(WorkFrontField.NAME.ToAPIString());
            pfields.Add(WorkFrontField.DESCRIPTION.ToAPIString());
            pfields.Add(WorkFrontField.WORKFRONTPROJECTSTATUS.ToAPIString());
            pfields.Add(WorkFrontField.PROJECTEDCOMPLETIONDATE.ToAPIString());
            pfields.Add(WorkFrontField.BUDGET.ToAPIString());
            return pfields;
        }

        public static List<WorkFrontField> GetWorkFrontFieldDetails()
        {
            List<WorkFrontField> fieldDetails = new List<WorkFrontField>();
            fieldDetails.Add(WorkFrontField.NAME);
            fieldDetails.Add(WorkFrontField.DESCRIPTION);
            fieldDetails.Add(WorkFrontField.WORKFRONTPROJECTSTATUS);
            fieldDetails.Add(WorkFrontField.ACTUALBENEFIT);
            fieldDetails.Add(WorkFrontField.ACTUALCOST);
            fieldDetails.Add(WorkFrontField.ACTUALDURATIONMINUTES);
            fieldDetails.Add(WorkFrontField.PROJECTEDCOMPLETIONDATE);
            fieldDetails.Add(WorkFrontField.BUDGET);
            fieldDetails.Add(WorkFrontField.DURATIONMINUTES);
            return fieldDetails;
        }
       
        public class WorkFrontField
        {
            public static readonly WorkFrontField NAME = new WorkFrontField("name", "Name", "string", true);
            public static readonly WorkFrontField DESCRIPTION = new WorkFrontField("description", "Description", "string", true);
            public static readonly WorkFrontField PLANNEDSTARTDATE = new WorkFrontField("plannedStartDate", "Planned Start Date", "date", false);
            public static readonly WorkFrontField WORKFRONTPROJECTSTATUS = new WorkFrontField("status", "WorkFront Project Status", "string", false);
            public static readonly WorkFrontField ACTUALBENEFIT = new WorkFrontField("actualBenefit", "Actual Benefit", "int", true);
            public static readonly WorkFrontField ACTUALCOST = new WorkFrontField("actualCost", "Actual Cost", "int", true);
            public static readonly WorkFrontField ACTUALDURATIONMINUTES = new WorkFrontField("actualDurationMinutes", "Actual Duration in Minutes", "int", true);
            public static readonly WorkFrontField PROJECTEDCOMPLETIONDATE = new WorkFrontField("projectedCompletionDate", "Projected Completion Date", "date", true);
            public static readonly WorkFrontField BUDGET = new WorkFrontField("budget", "Budget", "int", true);
            public static readonly WorkFrontField DURATIONMINUTES = new WorkFrontField("durationMinutes", "Duration in Minutes", "int", true);
            public static readonly WorkFrontField ACTUALSTARTDATE = new WorkFrontField("actualStartDate", "Actual Start Date", "date", true);
            public static readonly WorkFrontField ACTUALVALUE = new WorkFrontField("actualValue", "Actual Value", "double", true);
            public static readonly WorkFrontField ROI = new WorkFrontField("roi", "ROI", "double", true);
            public static readonly WorkFrontField SPONSORID = new WorkFrontField("sponsorID", "Sponsor ID", "string", false);
            public static readonly WorkFrontField OWNERID = new WorkFrontField("ownerID", "Owner ID", "string", false);

            /// <summary>
            /// String representation of the Fields
            /// </summary>
            public string apiString { get; private set; }
            public string value { get; private set; }
            public string dataType { get; private set; }
            public bool writeable { get; private set; }

            /// <summary>
            /// Creates a new Fields with the given value
            /// </summary>
            /// <param name="val">
            /// Object code value as needed for the object URI
            /// </param>
            private WorkFrontField(string fieldName, string fieldValue, string datatype, bool writeable)
            {
                this.apiString = fieldName;
                this.value = fieldValue;
                this.dataType = datatype;
                this.writeable = writeable;
            }

            /// <summary>
            /// Returns Fields.userString as a string - what WorkFront API looks for
            /// </summary>
            /// <returns>
            /// Returns Fields.userString as a string
            /// </returns>
            public override string ToString()
            {
                return value;
            }

            /// <summary>
            /// Returns the API string for the field - what WorkFront API looks for
            /// </summary>
            /// <returns>
            /// Returns Fields.apiField as a string
            /// </returns>
            public string ToAPIString()
            {
                return apiString;
            }

        }

        public class GameplanField
        {
            public static readonly GameplanField TITLE = new GameplanField("Title", "Title");
            public static readonly GameplanField DESCRIPTION = new GameplanField("Description", "Description");
            public static readonly GameplanField START_DATE = new GameplanField("StartDate", "Start Date");
            public static readonly GameplanField END_DATE = new GameplanField("EndDate", "End Date");
            public static readonly GameplanField PARENT_PROGRAM = new GameplanField("ParentProgram", "Parent Program");
            public static readonly GameplanField PARENT_CAMPAIGN = new GameplanField("ParentCampaign", "Parent Campaign");
            public static readonly GameplanField PROGRAM_OWNER = new GameplanField("ProgramOwner", "Program Owner");
            public static readonly GameplanField CAMPAIGN_OWNER = new GameplanField("CampaignOwner", "Campaign Owner");
            public static readonly GameplanField CAMPAIGN_START = new GameplanField("CampaignStartDate", "Campaign Start Date");
            public static readonly GameplanField CAMPAIGN_END = new GameplanField("CampaignEndDate", "Campaign End Date");
            public static readonly GameplanField PROGRAM_START = new GameplanField("ProgramStartDate", "Program Start Date");
            public static readonly GameplanField PROGRAM_END = new GameplanField("ProgramEndDate", "Program End Date");
			// Added by Arpita Soni for Ticket #2201 on 20/06/2016 to push program as an attribute of WF project
            public static readonly GameplanField PROGRAM_NAME = new GameplanField("ProgramName", "Program Name");


            /// <summary>
            /// String representation of the Fields
            /// </summary>
            private string apiString { get;  set; }
            private string value { get;  set; }

            /// <summary>
            /// Creates a new Fields with the given value
            /// </summary>
            /// <param name="val">
            /// Object code value as needed for the object URI
            /// </param>
            private GameplanField(string fieldName, string fieldValue)
            {
                this.apiString = fieldName;
                this.value = fieldValue;
            }

            /// <summary>
            /// Returns Fields.userString as a string - what WorkFront API looks for
            /// </summary>
            /// <returns>
            /// Returns Fields.userString as a string
            /// </returns>
            public override string ToString()
            {
                return value;
            }

            /// <summary>
            /// Returns the API string for the field - what WorkFront API looks for
            /// </summary>
            /// <returns>
            /// Returns Fields.apiField as a string
            /// </returns>
            public string ToAPIString()
            {
                return apiString;
            }

        }
    }
}
       

