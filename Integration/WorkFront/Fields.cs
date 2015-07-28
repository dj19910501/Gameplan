
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

        public enum GamePlanTacticFields
        {
            [Description("Title")]
            TITLE = 1,
            [Description("Description")]
            DESCRIPTION = 3,
            [Description("Start Date")]
            START_DATE = 5,
            [Description("End Date")]
            END_DATE = 7,
            [Description("Cost")]
            COST = 10,
            [Description("Tactic Budget")]
            TACTIC_BUDGET = 12,
            [Description("Tactic Status")]
            STATUS = 14
        }

        public static List<string> ReturnAllWorkFrontFields_AsUserText()
        {
            List<string> temp = new List<string>();
            temp.Add(WorkFrontField.NAME.ToString());
            temp.Add(WorkFrontField.DESCRIPTION.ToString());
            temp.Add(WorkFrontField.WORKFRONTPROJECTSTATUS.ToString());
            temp.Add(WorkFrontField.PLANNEDSTARTDATE.ToString());
            temp.Add(WorkFrontField.ACTUALCOST.ToString());
            temp.Add(WorkFrontField.ACTUALBENEFIT.ToString());
            temp.Add(WorkFrontField.ACTUALDURATIONMINUTES.ToString());
            temp.Add(WorkFrontField.ACTUALCOMPLETIONDATE.ToString());
            temp.Add(WorkFrontField.BUDGET.ToString());
            temp.Add(WorkFrontField.DURATIONMINUTES.ToString());
            return temp;
        }

        public static List<string> ReturnAllWorkFrontFields_AsAPI()
        {
            List<string> temp = new List<string>();
            temp.Add(WorkFrontField.NAME.ToAPIString());
            temp.Add(WorkFrontField.DESCRIPTION.ToAPIString());
            temp.Add(WorkFrontField.WORKFRONTPROJECTSTATUS.ToAPIString());
            temp.Add(WorkFrontField.PLANNEDSTARTDATE.ToAPIString());
            temp.Add(WorkFrontField.ACTUALCOST.ToAPIString());
            temp.Add(WorkFrontField.ACTUALBENEFIT.ToAPIString());
            temp.Add(WorkFrontField.ACTUALDURATIONMINUTES.ToAPIString());
            temp.Add(WorkFrontField.ACTUALCOMPLETIONDATE.ToAPIString());
            temp.Add(WorkFrontField.BUDGET.ToAPIString());
            temp.Add(WorkFrontField.DURATIONMINUTES.ToAPIString());
            return temp;
        }

        public static List<WorkFrontField> GetWorkFrontFieldDetails()
        {
            List<WorkFrontField> fieldDetails = new List<WorkFrontField>();
            fieldDetails.Add(WorkFrontField.NAME);
            fieldDetails.Add(WorkFrontField.DESCRIPTION);
            fieldDetails.Add(WorkFrontField.PLANNEDSTARTDATE);
            fieldDetails.Add(WorkFrontField.WORKFRONTPROJECTSTATUS);
            fieldDetails.Add(WorkFrontField.ACTUALBENEFIT);
            fieldDetails.Add(WorkFrontField.ACTUALCOST);
            fieldDetails.Add(WorkFrontField.ACTUALDURATIONMINUTES);
            fieldDetails.Add(WorkFrontField.ACTUALCOMPLETIONDATE);
            fieldDetails.Add(WorkFrontField.BUDGET);
            fieldDetails.Add(WorkFrontField.DURATIONMINUTES);
            return fieldDetails;
        }
       
        public class WorkFrontField
        {
            public static readonly WorkFrontField NAME = new WorkFrontField("name", "Name", "string", true);
            public static readonly WorkFrontField DESCRIPTION = new WorkFrontField("description", "Description", "string", true);
            public static readonly WorkFrontField PLANNEDSTARTDATE = new WorkFrontField("plannedStartDate", "Planned Start Date", "date", true);
            public static readonly WorkFrontField WORKFRONTPROJECTSTATUS = new WorkFrontField("WorkFront Project Status", "WorkFront Project Status", "string", false);
            public static readonly WorkFrontField ACTUALBENEFIT = new WorkFrontField("actualBenefit", "Actual Benefit", "int", true);
            public static readonly WorkFrontField ACTUALCOST = new WorkFrontField("actualCost", "Actual Cost", "int", true);
            public static readonly WorkFrontField ACTUALDURATIONMINUTES = new WorkFrontField("actualDurationMinutes", "Actual Duration in Minutes", "int", true);
            public static readonly WorkFrontField ACTUALCOMPLETIONDATE = new WorkFrontField("actualCompletionDate", "Actual Completion Date", "date", true);
            public static readonly WorkFrontField BUDGET = new WorkFrontField("budget", "Budget", "int", true);
            public static readonly WorkFrontField DURATIONMINUTES = new WorkFrontField("durationMinutes", "Duration in Minutes", "int", true);

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

            /// <summary>
            /// String representation of the Fields
            /// </summary>
            public string apiString { get; private set; }
            public string value { get; private set; }

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
       

