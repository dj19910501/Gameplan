﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class CurrencyModel
    {
       
        public int CurrencyId { get; set; }
    
        public string CurrencySymbol { get; set; }
      
        public string ISOCurrencyCode { get; set; }
      
        public string CurrencyDetail { get; set; }
       
        public bool IsDeleted { get; set; }
      
        //public Guid CreatedBy { get; set; }
      
        //public DateTime CreatedDate { get; set; }
        public long ClientCurrencyId { get; set; }
     
        public Guid ClientId { get; set; }
        public Guid UserId { get; set; }
        public bool IsDefault { get; set; }
	    // Add By Nishant Sheth
        public class ClientCurrency
        {
            public string CurrencyCode { get; set; }
            public string CurrencySymbol { get; set; }
            public System.Guid ClientId { get; set; }
            public System.DateTime StartDate { get; set; }
            public System.DateTime EndDate { get; set; }
            public double ExchangeRate { get; set; }
            public string Component { get; set; }
        }

        public class PlanCurrency
        {
            public string CurrencyCode { get; set; }
            public string CurrencySymbol { get; set; }
            public double ExchangeRate { get; set; }
        }
        //End By Nishant Sheth
    }
}