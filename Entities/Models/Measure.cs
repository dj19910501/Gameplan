//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RevenuePlanner.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Measure
    {
        public Measure()
        {
            this.Goals = new HashSet<Goal>();
            this.GoalDistributions = new HashSet<GoalDistribution>();
            this.KeyDatas = new HashSet<KeyData>();
            this.MeasureOutputValues = new HashSet<MeasureOutputValue>();
            this.ReportGraphColumns = new HashSet<ReportGraphColumn>();
            this.ReportGraphColumns1 = new HashSet<ReportGraphColumn>();
            this.ReportGraphColumns2 = new HashSet<ReportGraphColumn>();
            this.ReportGraphColumns3 = new HashSet<ReportGraphColumn>();
            this.ReportGraphColumns4 = new HashSet<ReportGraphColumn>();
            this.ReportTableColumns = new HashSet<ReportTableColumn>();
        }
    
        public int id { get; set; }
        public string Name { get; set; }
        public string AggregationQuery { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string MeasureTableName { get; set; }
        public string AggregationType { get; set; }
        public Nullable<bool> DisplayColorIndication { get; set; }
        public Nullable<bool> ComputeAllValues { get; set; }
        public string ComputeAllValuesFormula { get; set; }
        public string DrillDownWhereClause { get; set; }
        public Nullable<bool> UseRowCountFromFormula { get; set; }
        public Nullable<bool> IsCurrency { get; set; }
        public string BaseCurrencyCode { get; set; }
    
        public virtual ICollection<Goal> Goals { get; set; }
        public virtual ICollection<GoalDistribution> GoalDistributions { get; set; }
        public virtual ICollection<KeyData> KeyDatas { get; set; }
        public virtual ICollection<MeasureOutputValue> MeasureOutputValues { get; set; }
        public virtual ICollection<ReportGraphColumn> ReportGraphColumns { get; set; }
        public virtual ICollection<ReportGraphColumn> ReportGraphColumns1 { get; set; }
        public virtual ICollection<ReportGraphColumn> ReportGraphColumns2 { get; set; }
        public virtual ICollection<ReportGraphColumn> ReportGraphColumns3 { get; set; }
        public virtual ICollection<ReportGraphColumn> ReportGraphColumns4 { get; set; }
        public virtual ICollection<ReportTableColumn> ReportTableColumns { get; set; }
    }
}
