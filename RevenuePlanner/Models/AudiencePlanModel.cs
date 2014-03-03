using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class AudiencePlanModel
    {

        public int ModelId { get; set; }
        public string Quarter { get; set; }

        #region Outbound Properties


        public int Q1UsableDatabase { get; set;}
        public int AllQOrigInboundInquiriesTotal { get; set; }

        public double NormalErosion { get; set; }
        public double UnsubscribeRate { get; set; }
        public double CTRDelivered { get; set; }
        public double RegistrationRate { get; set; }



        public double Q1NormalErosion { get; set; }
        public double Q1UnsubscribeRate { get; set; }
        public double Q1CTRDelivered { get; set; }
    

        public double Q1OutBoundErosion { get; set; }
        public double Q1OutBoundUnsubscribeRate { get; set; }
        public double Q1OutboundCTRDelivered { get; set; }
        public double Q1RegistrationRate { get; set; }

        public int NumberOfTouchesQ1 { get; set; }
        public int NumberOfTouchesQ2 { get; set; }
        public int NumberOfTouchesQ3 { get; set; }
        public int NumberOfTouchesQ4 { get; set; }
        public int ListAcquisitionsQ1 { get; set; }
        public int ListAcquisitionsQ2  { get; set; }
        public int ListAcquisitionsQ3 { get; set; }
        public int ListAcquisitionsQ4 { get; set; }

        //public float Acquisition_CostperContactQ1 { get; set; }
        //public float Acquisition_CostperContactQ2 { get; set; }
        //public float Acquisition_CostperContactQ3 { get; set; }
        //public float Acquisition_CostperContactQ4 { get; set; }

        public double Acquisition_CostperContactQ1 { get; set; }
        

        public int Acquisition_NumberofTouchesQ1 { get; set; }
        public int Acquisition_NumberofTouchesQ2 { get; set; }
        public int Acquisition_NumberofTouchesQ3 { get; set; }
        public int Acquisition_NumberofTouchesQ4 { get; set; }
        public double Acquisition_RegistrationRate { get; set; }




        #endregion

        #region Inbound Properties

       
        public int Impressions { get; set; }
        public double ClickThroughRate { get; set; }
        public int Visits { get; set; }
        public double InboundRegistrationRate { get; set; }
        public int PPC_ClickThroughs { get; set; }
        public double PPC_CostperClickThrough { get; set; }
        public double PPC_RegistrationRate { get; set; }
        public int GC_GuaranteedCPLBudget { get; set; }
        public int GC_CostperLead { get; set; }
        public int CSC_NonGuaranteedCPLBudget { get; set; }
        public double CSC_CostperLead { get; set; }
        public int TDM_DigitalMediaBudget { get; set; }
        public double TDM_CostperLead { get; set; }
        public int TP_PrintMediaBudget { get; set; }
        public double TP_CostperLead { get; set; }

        #endregion

        #region Events Properties

        public int NumberofContactsQ1 { get; set; }
        public int NumberofContactsQ2 { get; set; }
        public int NumberofContactsQ3 { get; set; }
        public int NumberofContactsQ4 { get; set; }
        public double ContactToInquiryConversion { get; set; }
        public double EventsBudgetQ1 { get; set; }
        public double EventsBudgetQ2 { get; set; }
        public double EventsBudgetQ3 { get; set; }
        public double EventsBudgetQ4 { get; set; }

        #endregion
        public List<ModelVersion> Versions { get; set; }
    }
}