using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class SyncFrequencyModel
    {
        public int IntegrationInstanceId { get; set; }
        public string Frequency { get; set; }
        public string Time { get; set; }
        public string DayofWeek { get; set; }

        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Day must be a natural number")]
        [MaxLength(2)]
        public string Day { get; set; }

        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
    }
}