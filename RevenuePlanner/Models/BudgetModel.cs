﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class BudgetMonth
    {
        public double Jan { get; set; } //Q1
        public double Feb { get; set; }
        public double Mar { get; set; }
        public double Apr { get; set; } //Q2
        public double May { get; set; }
        public double Jun { get; set; }
        public double Jul { get; set; } //Q3
        public double Aug { get; set; }
        public double Sep { get; set; }
        public double Oct { get; set; } //Q4
        public double Nov { get; set; }
        public double Dec { get; set; }
    }
    public class BudgetModel
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public bool IsOwner { get; set; }

        public BudgetMonth Month { get; set; }
        public BudgetMonth SumMonth { get; set; }
        public BudgetMonth ParentMonth { get; set; }

        public double Allocated { get; set; }
        public double Budgeted { get; set; }

        public int ParentActivityId { get; set; }
    }
    public class BudgetedValue
    {
        public string Period { get; set; }
        public double Value { get; set; }
    }
}