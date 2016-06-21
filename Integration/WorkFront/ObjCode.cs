
namespace Integration.WorkFront
{
    /// <summary>
	/// Creates a type-safe representation of the ObjCodes used in the Stream API
    /// Each object in WorkFront is given a unique URI consisting of the object type and 
    /// the URI. Example: '/attask/api/proj/4c78821c0000d6fa8d5e52f07a1d54d0'
    /// More information: https://developers.attask.com/api-docs/api-explorer/
	/// </summary>
	public class ObjCode {
		public static readonly ObjCode PROJECT 				= new ObjCode("proj");
		public static readonly ObjCode TASK 				= new ObjCode("task");
		public static readonly ObjCode ISSUE 				= new ObjCode("optask");
		public static readonly ObjCode OPTASK 				= new ObjCode("optask");
		public static readonly ObjCode TEAM 				= new ObjCode("team");
		public static readonly ObjCode HOUR					= new ObjCode("hour");
		public static readonly ObjCode TIMESHEET			= new ObjCode("tshet");
		public static readonly ObjCode USER					= new ObjCode("user");
		public static readonly ObjCode ASSIGNMENT			= new ObjCode("assgn");
		public static readonly ObjCode USER_PREF			= new ObjCode("userpf");
		public static readonly ObjCode CATEGORY				= new ObjCode("ctgy");
		public static readonly ObjCode CATEGORY_PARAMETER 	= new ObjCode("ctgypa");
		public static readonly ObjCode PARAMETER			= new ObjCode("param");
		public static readonly ObjCode PARAMETER_GROUP		= new ObjCode("pgrp");
		public static readonly ObjCode PARAMETER_OPTION		= new ObjCode("popt");
		public static readonly ObjCode PARAMETER_VALUE		= new ObjCode("pval");
		public static readonly ObjCode ROLE					= new ObjCode("role");
		public static readonly ObjCode GROUP				= new ObjCode("group");
		public static readonly ObjCode NOTE					= new ObjCode("note");
		public static readonly ObjCode DOCUMENT				= new ObjCode("docu");
		public static readonly ObjCode DOCUMENT_VERSION 	= new ObjCode("docv");
		public static readonly ObjCode EXPENSE				= new ObjCode("expns");
		public static readonly ObjCode CUSTOM_ENUM			= new ObjCode("custem");
        public static readonly ObjCode TEMPLATE             = new ObjCode("tmpl");
        public static readonly ObjCode PORTFOLIO            = new ObjCode("portfolio");
        public static readonly ObjCode QUEUE                = new ObjCode("QUED");
		// Added by Arpita Soni for Ticket #2201 on 06/20/2016
        public static readonly ObjCode PROGRAM              = new ObjCode("prgm");
		
		/// <summary>
		/// String representation of the ObjCode
		/// </summary>
		public string Value { get; private set; }
		
		/// <summary>
		/// Creates a new ObjCode with the given value
		/// </summary>
		/// <param name="val">
		/// Object code value as needed for the object URI
		/// </param>
        private ObjCode(string val)
        {
			this.Value = val;
		}
		
		/// <summary>
		/// Returns ObjCode.Value as a string
		/// </summary>
		/// <returns>
        /// Returns ObjCode.Value as a string
		/// </returns>
		public override string ToString() {
			return Value;
		}
		
		/// <summary>
		/// Compares this.Value to the given value.
		/// This means that:  myObjCode.equals(ObjCode.NOTE)is the same as: myObjCode.equals("note");
		/// </summary>
		/// <param name="obj">
		/// The given value for comparison as an object
		/// </param>
		/// <returns>
		/// A boolean: True if the objects represent the same ObjCode.
		/// </returns>
		public override bool Equals(object obj) {
			if(obj != null && obj is string) {
				return Value.Equals((obj as string).ToLower());
			}
			return Value.Equals(obj);
		}
		
		/// <summary>
		/// Returns the hash code of Value
		/// </summary>
		public override int GetHashCode() {
			return Value.GetHashCode();
		}

    }
}
