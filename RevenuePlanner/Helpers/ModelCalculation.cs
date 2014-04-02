using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Helpers
{
    public class ModelCalculation
    {
        #region Variables

        private int _ModelId;
        public int ModelId
        {
            get
            {
                return _ModelId;
            }
            set
            {
                _ModelId = value;
            }
        }

        MRPEntities db = new MRPEntities();
        double CurrentBaseline, ConversionGate, AverageDealSize, StageVelocity, InboundInquiries, Value;
        double MSUSINQC, MINQAQLC, MAQLTALC, MTALTQLC, MTQLSALC, MSALSQLC, MSQLCWC, MDSZ, MOINQL, MIINQL, MAQLL, MTALL, MTQLL, MSALL, MSQLL, Ifactor, CurrentGap, Improvement, BIC = 2.00;
        double ITGL, TTGLSALC, TSALSQLC, TSQLCWC, TDSZ, TTQLL, TSQLL, Tele_SAL_Stage, Tele_SQL_Stage;
        double ISGL, SSGLSQLC, SSQLCWC, SDSZ, SSALL, SSQLL, Sales_SAL_Stage, Sales_SQL_Stage;
        static int Count = 0;
        static int Weight = 0;

        #region Stage
        static string SUS = Convert.ToString(Enums.Stage.SUS).ToLower();
        static string INQ = Convert.ToString(Enums.Stage.INQ).ToLower();
        static string AQL = Convert.ToString(Enums.Stage.AQL).ToLower();
        static string TAL = Convert.ToString(Enums.Stage.TAL).ToLower();
        static string TQL = Convert.ToString(Enums.Stage.TQL).ToLower();
        static string SAL = Convert.ToString(Enums.Stage.SAL).ToLower();
        static string SQL = Convert.ToString(Enums.Stage.SQL).ToLower();
        static string CW = Convert.ToString(Enums.Stage.CW).ToLower();
        #endregion

        #region Stage Type
        static string CR = Convert.ToString(Enums.StageType.CR).ToLower();
        static string SV = Convert.ToString(Enums.StageType.SV).ToLower();
        #endregion

        #region Funnel
        static string Marketing = Convert.ToString(Enums.Funnel.Marketing).ToLower();
        static string Teleprospecting = Convert.ToString(Enums.Funnel.Teleprospecting).ToLower();
        static string Sales = Convert.ToString(Enums.Funnel.Sales).ToLower();
        #endregion

        #region Funnel Field
        static string FF_DatabaseSize = Convert.ToString(Enums.FunnelField.DatabaseSize).ToLower();
        static string ConversionGate_SUS = Convert.ToString(Enums.FunnelField.ConversionGate_SUS).ToLower();
        static string FF_OutboundGeneratedInquiries = Convert.ToString(Enums.FunnelField.OutboundGeneratedInquiries).ToLower();
        static string FF_InboundInquiries = Convert.ToString(Enums.FunnelField.InboundInquiries).ToLower();
        static string FF_TotalInquiries = Convert.ToString(Enums.FunnelField.TotalInquiries).ToLower();
        static string ConversionGate_INQ = Convert.ToString(Enums.FunnelField.ConversionGate_INQ).ToLower();
        static string FF_AQL = Convert.ToString(Enums.FunnelField.AQL).ToLower();
        static string ConversionGate_AQL = Convert.ToString(Enums.FunnelField.ConversionGate_AQL).ToLower();
        static string FF_TAL = Convert.ToString(Enums.FunnelField.TAL).ToLower();
        static string ConversionGate_TAL = Convert.ToString(Enums.FunnelField.ConversionGate_TAL).ToLower();
        static string FF_TQL = Convert.ToString(Enums.FunnelField.TQL).ToLower();
        static string FF_TGL = Convert.ToString(Enums.FunnelField.TGL).ToLower();
        static string FF_MQL = Convert.ToString(Enums.FunnelField.MQL).ToLower();
        static string ConversionGate_TQL = Convert.ToString(Enums.FunnelField.ConversionGate_TQL).ToLower();
        static string FF_SAL = Convert.ToString(Enums.FunnelField.SAL).ToLower();
        static string FF_SGL = Convert.ToString(Enums.FunnelField.SGL).ToLower();
        static string ConversionGate_SAL = Convert.ToString(Enums.FunnelField.ConversionGate_SAL).ToLower();
        static string FF_SQL = Convert.ToString(Enums.FunnelField.SQL).ToLower();
        static string ConversionGate_SQL = Convert.ToString(Enums.FunnelField.ConversionGate_SQL).ToLower();
        static string ClosedWon = Convert.ToString(Enums.FunnelField.ClosedWon).ToLower();
        static string BlendedFunnelCR_Times = Convert.ToString(Enums.FunnelField.BlendedFunnelCR_Times).ToLower();
        static string FF_AverageDealsSize = Convert.ToString(Enums.FunnelField.AverageDealsSize).ToLower();
        static string ExpectedRevenue = Convert.ToString(Enums.FunnelField.ExpectedRevenue).ToLower();
        #endregion

        #region Quarter
        static string Q1 = Convert.ToString(Enums.Quarter.Q1).ToLower();
        static string Q2 = Convert.ToString(Enums.Quarter.Q2).ToLower();
        static string Q3 = Convert.ToString(Enums.Quarter.Q3).ToLower();
        static string Q4 = Convert.ToString(Enums.Quarter.Q4).ToLower();
        static string ALLQ = Convert.ToString(Enums.Quarter.ALLQ).ToLower();
        #endregion

        #endregion

        #region Misc Calculation

        public double TotalInboundInquiries(string quarter)
        {
            double TotalInboundInquiries = 0;
            if (!String.IsNullOrWhiteSpace(quarter))
            {
                double TotalUnpaidInboundInquiries, NumberofModeledInquires_PPC, NumberofModeledInquiries_GCPL, NumberofModeledInquiries_NonGCPL, NumberofModeledInquiries_TDM, NumberofModeledInquiries_TP, NumberofInquiries_PhyEve;
                double TotalOrganicInquiries, PlusOriginalInboundInquiries, Visits_SEO;

                Visits_SEO = (Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.ClickThroughRate).FirstOrDefault()) / 100) * Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.Impressions).FirstOrDefault());
                PlusOriginalInboundInquiries = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Marketing).Select(m => m.ExpectedLeadCount).FirstOrDefault() / 4;
                TotalOrganicInquiries = Visits_SEO * (Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.RegistrationRate).FirstOrDefault()) / 100);

                TotalUnpaidInboundInquiries = TotalOrganicInquiries + PlusOriginalInboundInquiries;
                NumberofModeledInquires_PPC = Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.PPC_ClickThroughs).FirstOrDefault()) * Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.PPC_RegistrationRate).FirstOrDefault());
                NumberofModeledInquiries_GCPL = Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.GC_GuaranteedCPLBudget).FirstOrDefault()) / Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.GC_CostperLead).FirstOrDefault());
                NumberofModeledInquiries_NonGCPL = Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.CSC_NonGuaranteedCPLBudget).FirstOrDefault()) / Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.CSC_CostperLead).FirstOrDefault());
                NumberofModeledInquiries_TDM = Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.TDM_DigitalMediaBudget).FirstOrDefault()) / Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.TDM_CostperLead).FirstOrDefault());
                NumberofModeledInquiries_TP = Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.TP_PrintMediaBudget).FirstOrDefault()) / Convert.ToDouble(db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == ALLQ).Select(m => m.TP_CostperLead).FirstOrDefault());
                NumberofInquiries_PhyEve = Convert.ToDouble(db.Model_Audience_Event.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == quarter).Select(m => m.NumberofContacts).FirstOrDefault()) * Convert.ToDouble(db.Model_Audience_Event.Where(m => m.ModelId == ModelId && m.Quarter.ToLower() == quarter).Select(m => m.ContactToInquiryConversion).FirstOrDefault());

                TotalInboundInquiries = TotalUnpaidInboundInquiries + NumberofModeledInquires_PPC + NumberofModeledInquiries_GCPL + NumberofModeledInquiries_NonGCPL + NumberofModeledInquiries_TDM + NumberofModeledInquiries_TP + NumberofInquiries_PhyEve;
            }
            return TotalInboundInquiries;
        }

        public double IFactor()
        {
            double IFactor1 = 0;
            double IFactor2 = 0;

            if (Count == 0)
            {
                IFactor1 = 0;
            }
            else if ((Weight / Count) < 2)
            {
                IFactor1 = 0.25;
            }
            else if ((Weight / Count) < 3)
            {
                IFactor1 = 0.50;
            }
            else if ((Weight / Count) < 4)
            {
                IFactor1 = 0.75;
            }
            else
            {
                IFactor1 = 0.9;
            }

            if (Count == 0)
            {
                IFactor2 = 0;
            }
            else if (Count < 3)
            {
                IFactor2 = 0.4;
            }
            else if (Count < 5)
            {
                IFactor2 = 0.6;
            }
            else if (Count < 8)
            {
                IFactor2 = 0.8;
            }
            else
            {
                IFactor2 = 1;
            }
            return IFactor1 * IFactor2;
        }

        /// <summary>
        /// Function to get number of modeled inquires without considering event.
        /// </summary>
        /// <param name="modelId">Model Id.</param>
        /// <returns>Returns number of modeled inquires without considering event.</returns>
        private double GetNumberOfModeledInquiriesWithoutEvent(int modelId)
        {
            string allQuarter = Enums.Quarter.ALLQ.ToString();
            Model_Audience_Inbound modelAudienceInbound = (Model_Audience_Inbound)db.Model_Audience_Inbound.Where(mai => mai.ModelId.Equals(modelId) && mai.Quarter.Equals(allQuarter)).FirstOrDefault();
            double numberOfModeledInquiriesWithoutEvent = 0;
            if (modelAudienceInbound != null)
            {
                double totalUnpaidInboundInquiries = 0;

                #region "Inbound Unpaid SEO (website + social media)"
                {
                    double visits = 0;
                    if (modelAudienceInbound.Impressions.HasValue && modelAudienceInbound.ClickThroughRate.HasValue)
                    {
                        visits = (double)(modelAudienceInbound.Impressions * (modelAudienceInbound.ClickThroughRate / 100));
                    }

                    double totalOrganicInquiries = 0;
                    if (visits != 0 && modelAudienceInbound.RegistrationRate.HasValue)
                    {
                        totalOrganicInquiries = (double)(visits * (modelAudienceInbound.RegistrationRate / 100));
                    }

                    string marketing = Enums.Funnel.Marketing.ToString();
                    Model_Funnel modelFunnel = (Model_Funnel)db.Model_Funnel.Single(mf => mf.ModelId.Equals(modelId) && mf.Funnel.Title.Equals(marketing));
                    double plusOriginalInboundInquiries = 0;
                    if (modelFunnel.ExpectedLeadCount != 0)
                    {
                        plusOriginalInboundInquiries = modelFunnel.ExpectedLeadCount / (double)4;
                    }

                    totalUnpaidInboundInquiries = totalOrganicInquiries + plusOriginalInboundInquiries;
                }
                #endregion

                #region "Inbound Paid Search (PPC)"
                double numberOfModeledInquiriesPPC = 0;
                if (modelAudienceInbound.PPC_ClickThroughs.HasValue && modelAudienceInbound.PPC_RegistrationRate.HasValue)
                {
                    numberOfModeledInquiriesPPC = (double)(modelAudienceInbound.PPC_ClickThroughs * (modelAudienceInbound.PPC_RegistrationRate / 100));
                }
                #endregion

                #region "Inbound Paid Media - Guaranteed CPL Programs"
                double numberOfModeledInquiriesGC = 0;
                if (modelAudienceInbound.GC_GuaranteedCPLBudget.HasValue && modelAudienceInbound.GC_CostperLead.HasValue && modelAudienceInbound.GC_CostperLead > 0)
                {
                    numberOfModeledInquiriesGC = (double)(modelAudienceInbound.GC_GuaranteedCPLBudget / modelAudienceInbound.GC_CostperLead);
                }
                #endregion

                #region "Inbound Paid Media - Non-Guaranteed Content Syndication CPL"
                double numberOfModeledInquiriesCSC = 0;
                if (modelAudienceInbound.CSC_NonGuaranteedCPLBudget.HasValue && modelAudienceInbound.CSC_CostperLead.HasValue && modelAudienceInbound.CSC_CostperLead > 0)
                {
                    numberOfModeledInquiriesCSC = (double)(modelAudienceInbound.CSC_NonGuaranteedCPLBudget / modelAudienceInbound.CSC_CostperLead);
                }
                #endregion

                #region "Inbound Paid Media - Traditional Digital Media"
                double numberOfModeledInquiriesTDM = 0;
                if (modelAudienceInbound.TDM_DigitalMediaBudget.HasValue && modelAudienceInbound.TDM_CostperLead.HasValue && modelAudienceInbound.TDM_CostperLead > 0)
                {
                    numberOfModeledInquiriesTDM = (double)(modelAudienceInbound.TDM_DigitalMediaBudget / modelAudienceInbound.TDM_CostperLead);
                }
                #endregion

                #region "Inbound Paid Media - Traditional Print"
                double numberOfModeledInquiriesTP = 0;
                if (modelAudienceInbound.TP_PrintMediaBudget.HasValue && modelAudienceInbound.TP_CostperLead.HasValue && modelAudienceInbound.TP_CostperLead > 0)
                {
                    numberOfModeledInquiriesTP = (double)(modelAudienceInbound.TP_PrintMediaBudget / modelAudienceInbound.TP_CostperLead);
                }
                #endregion;

                numberOfModeledInquiriesWithoutEvent = totalUnpaidInboundInquiries + numberOfModeledInquiriesPPC + numberOfModeledInquiriesGC + numberOfModeledInquiriesCSC + numberOfModeledInquiriesTDM + numberOfModeledInquiriesTP;
            }

            return numberOfModeledInquiriesWithoutEvent;
        }

        /// <summary>
        /// Function to get number of modeled inquiries event.
        /// </summary>
        /// <param name="modelId">Model Id.</param>
        /// <param name="quarter">Quarter i.e. Q1, Q2, Q3 and Q4.</param>
        /// <returns>Returns number of modeled inquiries event.</returns>
        private double GetNumberOfModeledInquiriesEvent(int modelId, string quarter)
        {
            #region "Physical Events (Roadshows, Tradeshows, other events)"
            Model_Audience_Event modelAudienceEvent = (Model_Audience_Event)db.Model_Audience_Event.Where(mae => mae.ModelId.Equals(modelId) && mae.Quarter.Equals(quarter)).FirstOrDefault();
            double numberOfModeledInquiriesEvent = 0;
            if (modelAudienceEvent != null)
            {
                if (modelAudienceEvent.NumberofContacts.HasValue && modelAudienceEvent.ContactToInquiryConversion.HasValue)
                {
                    numberOfModeledInquiriesEvent = (double)(modelAudienceEvent.NumberofContacts * modelAudienceEvent.ContactToInquiryConversion);
                }
            }

            return numberOfModeledInquiriesEvent;
            #endregion
        }

        /// <summary>
        /// Function to get DBSizeByQuarter.
        /// </summary>
        /// <param name="usableSizeOfDatabase">Usable size of database.</param>
        /// <param name="modelAudienceOutboundQ1">Audience outbound for Q1.</param>
        /// <param name="modelAudienceOutbound">Audience outbound for current quarter.</param>
        /// <param name="totalInboundInquiriesSummary">Total inbound inquiries summary.</param>
        /// <returns>Returns Database size by quarter.</returns>
        private double GetDBSizeByQuarter(double usableSizeOfDatabase, Model_Audience_Outbound modelAudienceOutboundQ1, Model_Audience_Outbound modelAudienceOutbound, double totalInboundInquiriesSummary)
        {
            double normalErosion = 0;
            double unsubscribeRate = 0;
            double numberofTouches = 0;

            if (modelAudienceOutboundQ1 != null)
            {
                normalErosion = Convert.ToDouble(modelAudienceOutboundQ1.NormalErosion);
                unsubscribeRate = Convert.ToDouble(modelAudienceOutboundQ1.UnsubscribeRate);
            }

            if (modelAudienceOutbound != null)
            {
                numberofTouches = Convert.ToDouble(modelAudienceOutbound.NumberofTouches);
            }

            double databaseSize = usableSizeOfDatabase * (1 - (normalErosion / 100)) * (Math.Pow((1 - (unsubscribeRate / 100)), numberofTouches)) + totalInboundInquiriesSummary;
            return Math.Round(databaseSize);
        }

        /// <summary>
        /// Function to get DBSIZEPST.
        /// </summary>
        /// <param name="modelId">Model Id.</param>
        /// <returns>Returns DBSIZEPST.</returns>
        public double GetDBSIZEPST(int modelId)
        {
            double usableSizeOfDatabaseQ1 = 0;
            double usableSizeOfDatabaseQ2 = 0;
            double usableSizeOfDatabaseQ3 = 0;
            double usableSizeOfDatabaseQ4 = 0;
            double numberOfModeledInquiriesWithoutEvent = GetNumberOfModeledInquiriesWithoutEvent(modelId);

            //// Calculate Usable Size of Database for Q1
            {
                Model model = (Model)db.Models.Single(m => m.ModelId.Equals(modelId));
                usableSizeOfDatabaseQ1 = model.AddressableContacts;
            }

            Model_Audience_Outbound modelAudienceOutboundQ1 = null;

            //// Calculate Usable Size of Database for Q2
            if (usableSizeOfDatabaseQ1 != 0)
            {
                string quarter1 = Enums.Quarter.Q1.ToString();
                modelAudienceOutboundQ1 = (Model_Audience_Outbound)db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter1)).FirstOrDefault();
                double numberOfModeledInquiriesEventQ1 = GetNumberOfModeledInquiriesEvent(modelId, quarter1);
                usableSizeOfDatabaseQ2 = GetDBSizeByQuarter(usableSizeOfDatabaseQ1, modelAudienceOutboundQ1, modelAudienceOutboundQ1, (numberOfModeledInquiriesWithoutEvent + numberOfModeledInquiriesEventQ1));
            }

            //// Calculate Usable Size of Database for Q3
            if (usableSizeOfDatabaseQ2 != 0)
            {
                string quarter2 = Enums.Quarter.Q2.ToString();
                double numberOfModeledInquiriesEventQ2 = GetNumberOfModeledInquiriesEvent(modelId, quarter2);
                Model_Audience_Outbound modelAudienceOutbound = (Model_Audience_Outbound)db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter2)).FirstOrDefault();
                usableSizeOfDatabaseQ3 = GetDBSizeByQuarter(usableSizeOfDatabaseQ2, modelAudienceOutboundQ1, modelAudienceOutbound, (numberOfModeledInquiriesWithoutEvent + numberOfModeledInquiriesEventQ2));
            }

            //// Calculate Usable Size of Database for Q4
            if (usableSizeOfDatabaseQ3 != 0)
            {
                string quarter3 = Enums.Quarter.Q3.ToString();
                double numberOfModeledInquiriesEventQ3 = GetNumberOfModeledInquiriesEvent(modelId, quarter3);
                Model_Audience_Outbound modelAudienceOutbound = (Model_Audience_Outbound)db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter3)).FirstOrDefault();
                usableSizeOfDatabaseQ4 = GetDBSizeByQuarter(usableSizeOfDatabaseQ3, modelAudienceOutboundQ1, modelAudienceOutbound, (numberOfModeledInquiriesWithoutEvent + numberOfModeledInquiriesEventQ3));
            }

            return usableSizeOfDatabaseQ4;
        }

        /// <summary>
        /// Function to get DBACQPST.
        /// </summary>
        /// <param name="modelId">Model Id.</param>
        /// <returns>Returns DBACQPST.</returns>
        public double GetDBACQPST(int modelId)
        {
            double cumulativeListSizeQ1 = 0;
            double cumulativeListSizeQ2 = 0;
            double cumulativeListSizeQ3 = 0;
            double cumulativeListSizeQ4 = 0;
            string quarter1 = Enums.Quarter.Q1.ToString();
            string quarter2 = Enums.Quarter.Q2.ToString();
            string quarter3 = Enums.Quarter.Q3.ToString();
            string quarter4 = Enums.Quarter.Q4.ToString();
            double normalErosion = 0;
            double unsubscribeRate = 0;
            double numberofTouches = 0;
            double listAcquisitions = 0;

            //// Calculate Cumulative List Size for Q1
            cumulativeListSizeQ1 = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter1)).Select(s => s.ListAcquisitions).FirstOrDefault());

            //// Calculate Cumulative List Size for Q2
            normalErosion = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter1)).Select(s => s.NormalErosion).FirstOrDefault());
            unsubscribeRate = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter1)).Select(s => s.UnsubscribeRate).FirstOrDefault());
            numberofTouches = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter1)).Select(s => s.NumberofTouches).FirstOrDefault());
            listAcquisitions = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter2)).Select(s => s.ListAcquisitions).FirstOrDefault());
            cumulativeListSizeQ2 = cumulativeListSizeQ1 * (1 - normalErosion) * Math.Pow((1 - unsubscribeRate), numberofTouches) + listAcquisitions;

            normalErosion = 0;
            unsubscribeRate = 0;
            numberofTouches = 0;
            listAcquisitions = 0;

            //// Calculate Cumulative List Size for Q3
            normalErosion = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter2)).Select(s => s.NormalErosion).FirstOrDefault());
            unsubscribeRate = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter2)).Select(s => s.UnsubscribeRate).FirstOrDefault());
            numberofTouches = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter2)).Select(s => s.NumberofTouches).FirstOrDefault());
            listAcquisitions = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter3)).Select(s => s.ListAcquisitions).FirstOrDefault());
            cumulativeListSizeQ3 = cumulativeListSizeQ1 * (1 - normalErosion) * Math.Pow((1 - unsubscribeRate), numberofTouches) + listAcquisitions;

            normalErosion = 0;
            unsubscribeRate = 0;
            numberofTouches = 0;
            listAcquisitions = 0;

            //// Calculate Cumulative List Size Q4
            normalErosion = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter3)).Select(s => s.NormalErosion).FirstOrDefault());
            unsubscribeRate = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter3)).Select(s => s.UnsubscribeRate).FirstOrDefault());
            numberofTouches = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter3)).Select(s => s.NumberofTouches).FirstOrDefault());
            listAcquisitions = Convert.ToDouble(db.Model_Audience_Outbound.Where(mao => mao.ModelId.Equals(modelId) && mao.Quarter.Equals(quarter4)).Select(s => s.ListAcquisitions).FirstOrDefault());
            cumulativeListSizeQ4 = cumulativeListSizeQ1 * (1 - normalErosion) * Math.Pow((1 - unsubscribeRate), numberofTouches) + listAcquisitions;

            return cumulativeListSizeQ4;
        }

        #endregion

        #region Marketing

        #region MarketingSourced

        public double MarketingSourced_Mkt_DatabaseSize(bool isBaseline, double DBSIZEPST, double DBACQPST) //F4
        {
            if (isBaseline)
            {
                Value = Convert.ToDouble(db.Models.Where(m => m.ModelId == ModelId).Select(m => m.AddressableContacts).FirstOrDefault());
            }
            else
            {
                double DBSIZE = Convert.ToDouble(db.Models.Where(m => m.ModelId == ModelId).Select(m => m.AddressableContacts).FirstOrDefault());
                Value = ((DBSIZE + DBSIZEPST) / 2) + DBACQPST;
            }
            return Value;
        }
        public double MarketingSourced_Mkt_SUS_ConversionGate(bool isBaseline) //F5
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SUS && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MSUSINQC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SUS && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MSUSINQC;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MSUSINQC + Improvement;
            }
            return ConversionGate;
        }
        public double MarketingSourced_Mkt_OutboundGeneratedInquiries() //C6
        {
            double DBSize = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_DatabaseSize).Select(m => m.MarketingSourced).FirstOrDefault());
            double Mkt_SUS_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SUS).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = DBSize * Mkt_SUS_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Mkt_InboundInquiries(bool isBaseline) //F7
        {
            if (isBaseline)
            {
                InboundInquiries = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Marketing).Select(m => m.ExpectedLeadCount).FirstOrDefault();
            }
            else
            {
                InboundInquiries = TotalInboundInquiries(Q1) + TotalInboundInquiries(Q2) + TotalInboundInquiries(Q3) + TotalInboundInquiries(Q4);
            }
            return InboundInquiries;
        }
        public double MarketingSourced_Mkt_TotalInquiries() //C8
        {
            double OutboundGeneratedInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            double InboundInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            Value = OutboundGeneratedInquiries + InboundInquiries;
            return Value;
        }
        public double MarketingSourced_Mkt_INQ_ConversionGate(bool isBaseline) //F9
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == INQ && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MINQAQLC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == INQ && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MINQAQLC;
                if (MINQAQLC == 1)
                {
                    BIC = 1;
                }
                else
                {
                    BIC = 14.9;
                }
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MINQAQLC + Improvement;
            }
            return ConversionGate;
        }
        public double MarketingSourced_Mkt_AQL() //C10
        {
            double TotalInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_TotalInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            double INQ_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_INQ).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = TotalInquiries * INQ_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Mkt_AQL_ConversionGate(bool isBaseline) //F11
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == AQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MAQLTALC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == AQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MAQLTALC;
                if (MAQLTALC == 1)
                {
                    BIC = 1;
                }
                else
                {
                    if (MINQAQLC == 1)
                    {
                        BIC = 11.8;
                    }
                    else
                    {
                        BIC = 79;
                    }
                }
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MAQLTALC + Improvement;
            }
            return ConversionGate;
        }

        public double MarketingSourced_Tele_TAL() //C12
        {
            double AQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AQL).Select(m => m.MarketingSourced).FirstOrDefault());
            double AQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_AQL).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = AQL * AQL_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Tele_TAL_ConversionGate(bool isBaseline) //F13
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MTALTQLC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MTALTQLC;
                if (MTALTQLC == 1)
                {
                    BIC = 0;
                }
                else
                {
                    if (MAQLTALC == 1)
                    {
                        if (MINQAQLC == 1)
                        {
                            BIC = 9.3;
                        }
                        else
                        {
                            if (MAQLTALC == 1)
                            {
                                BIC = 62.4;
                            }
                            else
                            {
                                BIC = 79;
                            }
                        }
                    }
                    else
                    {
                        BIC = 79;
                    }
                }
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MTALTQLC + Improvement;
            }
            return ConversionGate;
        }
        public double MarketingSourced_Tele_TQL() //C14
        {
            double TAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TAL).Select(m => m.MarketingSourced).FirstOrDefault());
            double TAL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_TAL).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = TAL * TAL_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Tele_TotalMarketingQualifiedLeadsMQL() //C16
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TQL).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double MarketingSourced_Tele_TQL_ConversionGate(bool isBaseline) //F17
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MTQLSALC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MTQLSALC;
                if (MTQLSALC == 1)
                {
                    BIC = 1;
                }
                else
                {
                    if (MTALTQLC == 1)
                    {
                        if (MAQLTALC == 1)
                        {
                            if (MINQAQLC == 1)
                            {
                                BIC = 7.9;
                            }
                            else
                            {
                                if (MAQLTALC == 1)
                                {
                                    BIC = 53;
                                }
                                else
                                {
                                    BIC = 67.2;
                                }
                            }
                        }
                        else
                        {
                            BIC = 67.2;
                        }
                    }
                    else
                    {
                        BIC = 85;
                    }
                }
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MTQLSALC + Improvement;
            }
            return ConversionGate;
        }

        public double MarketingSourced_Sales_SAL() //C18
        {
            double MQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.MarketingSourced).FirstOrDefault());
            double TQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_TQL).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = MQL * TQL_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Sales_SAL_ConversionGate(bool isBaseline) //F20
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MSALSQLC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MSALSQLC;
                if (MSALSQLC == 1)
                {
                    BIC = 1;
                }
                else
                {
                    if (MTALTQLC == 1)
                    {
                        if (MAQLTALC == 1)
                        {
                            if (MINQAQLC == 1)
                            {
                                BIC = 4.88;
                            }
                            else
                            {
                                if (MAQLTALC == 1)
                                {
                                    BIC = 32.7;
                                }
                                else
                                {
                                    BIC = 41.4;
                                }
                            }
                        }
                        else
                        {
                            BIC = 41.4;
                        }
                    }
                    else
                    {
                        BIC = 61.7;
                    }
                }
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MSALSQLC + Improvement;
            }
            return ConversionGate;
        }
        public double MarketingSourced_Sales_SQL() //C21
        {
            double SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.MarketingSourced).FirstOrDefault());
            double SAL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SAL).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = SAL * SAL_ConversionGate;
            return Value;
        }
        public double MarketingSourced_Sales_SQL_ConversionGate(bool isBaseline) //F22
        {
            if (isBaseline)
            {
                ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MSQLCWC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MSQLCWC;
                BIC = 29.1;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                ConversionGate = MSQLCWC + Improvement;
            }
            return ConversionGate;
        }
        public double MarketingSourced_Sales_CW() //C23
        {
            double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.MarketingSourced).FirstOrDefault());
            double SQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SQL).Select(m => m.MarketingSourced).FirstOrDefault()) / 100;
            Value = SQL * SQL_ConversionGate;
            return Value;
        }

        public double MarketingSourced_Average_Deal_Size(bool isBaseline) //F25
        {
            if (isBaseline)
            {
                AverageDealSize = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Marketing).Select(m => m.AverageDealSize).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MDSZ = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Marketing).Select(m => m.AverageDealSize).FirstOrDefault();
                CurrentBaseline = MDSZ;
                BIC = 10;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                AverageDealSize = MDSZ * (1 + Improvement);
            }
            return AverageDealSize;
        }
        public double MarketingSourced_Expected_Revenue() //C26
        {
            double AverageDealsSize = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AverageDealsSize).Select(m => m.MarketingSourced).FirstOrDefault());
            double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.MarketingSourced).FirstOrDefault());
            Value = AverageDealsSize * CW;
            return Value;
        }

        #endregion

        #region MarketingStageDays

        public double MarketingStageDays_Mkt_OutboundGeneratedInquiries(bool isBaseline) //I6
        {
            if (isBaseline)
            {
                StageVelocity = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SUS && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MOINQL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SUS && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MOINQL;
                BIC = 10;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                StageVelocity = MOINQL - Improvement;
            }
            return StageVelocity;
        }
        public double MarketingStageDays_Mkt_InboundInquiries(bool isBaseline) //I7
        {
            if (isBaseline)
            {
                StageVelocity = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == INQ && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MIINQL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == INQ && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MIINQL;
                BIC = 10;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                StageVelocity = MIINQL - Improvement;
            }
            return StageVelocity;
        }
        public double MarketingStageDays_Mkt_TotalInquiries(double TotalInquiries = 0) //D8
        {
            if (TotalInquiries == 0)
            {
                return 0;
            }
            else
            {
                double InboundInquiries_MarketingSourced = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
                double InboundInquiries_MarketingDays = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.MarketingDays).FirstOrDefault());
                double OutboundGeneratedInquiries_MarketingSourced = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
                double OutboundGeneratedInquiries_MarketingDays = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.MarketingDays).FirstOrDefault());
                Value = (InboundInquiries_MarketingSourced / TotalInquiries) * InboundInquiries_MarketingDays + (OutboundGeneratedInquiries_MarketingSourced / TotalInquiries) * OutboundGeneratedInquiries_MarketingDays;
                return Value;
            }
        }
        public double MarketingStageDays_Mkt_AQL(bool isBaseline) //I10
        {
            if (isBaseline)
            {
                StageVelocity = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == AQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MAQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == AQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MAQLL;
                BIC = 2;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                StageVelocity = MAQLL - Improvement;
            }
            return StageVelocity;
        }

        public double MarketingStageDays_Tele_TAL(bool isBaseline) //I12
        {
            if (isBaseline)
            {
                StageVelocity = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == TAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                Ifactor = IFactor();
                MTALL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == TAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
                CurrentBaseline = MTALL;
                BIC = 2;
                CurrentGap = BIC - CurrentBaseline;
                if (CurrentGap > 0)
                {
                    Improvement = CurrentGap * Ifactor;
                }
                else
                {
                    Improvement = 0;
                }
                StageVelocity = MTALL - Improvement;
            }
            return StageVelocity;
        }
        public double MarketingStageDays_Tele_MQL() //I16
        {
            Ifactor = IFactor();
            MTQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            CurrentBaseline = MTQLL;
            BIC = 2;
            CurrentGap = BIC - CurrentBaseline;
            if (CurrentGap > 0)
            {
                Improvement = CurrentGap * Ifactor;
            }
            else
            {
                Improvement = 0;
            }
            StageVelocity = MTQLL - Improvement;
            return StageVelocity;
        }

        public double MarketingStageDays_Sales_SAL() //I18
        {
            Ifactor = IFactor();
            MSALL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            CurrentBaseline = MSALL;
            BIC = 5;
            CurrentGap = BIC - CurrentBaseline;
            if (CurrentGap > 0)
            {
                Improvement = CurrentGap * Ifactor;
            }
            else
            {
                Improvement = 0;
            }
            StageVelocity = MSALL - Improvement;
            return StageVelocity;
        }
        public double MarketingStageDays_Sales_SQL() //I21
        {
            Ifactor = IFactor();
            MSQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Marketing).Select(m => m.Value).FirstOrDefault();
            CurrentBaseline = MSQLL;
            BIC = 65;
            CurrentGap = BIC - CurrentBaseline;
            if (CurrentGap > 0)
            {
                Improvement = CurrentGap * Ifactor;
            }
            else
            {
                Improvement = 0;
            }
            StageVelocity = MSQLL - Improvement;
            return StageVelocity;
        }

        public double MarketingStageDays_BlendedFull() //D24
        {
            double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.MarketingDays).FirstOrDefault());
            double SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.MarketingDays).FirstOrDefault());
            double MQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.MarketingDays).FirstOrDefault());
            double TAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TAL).Select(m => m.MarketingDays).FirstOrDefault());
            double AQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AQL).Select(m => m.MarketingDays).FirstOrDefault());
            double TotalInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_TotalInquiries).Select(m => m.MarketingDays).FirstOrDefault());
            Value = SQL + SAL + MQL + TAL + AQL + TotalInquiries;
            return Value;
        }

        #endregion

        #endregion
         /*changed by Nirav Shah on 2 APR 2013*/
        //#region Teleprospecting

        //#region TeleprospectingSourced

        //public double TeleprospectingSourced_Tele_TGL(bool isBaseline) //N15
        //{
        //    if (isBaseline)
        //    {
        //        Value = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.ExpectedLeadCount).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        ITGL = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.ExpectedLeadCount).FirstOrDefault();
        //        CurrentBaseline = ITGL;
        //        BIC = 5;
        //        CurrentGap = BIC;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        Value = ITGL * (1 + Improvement);
        //    }

        //    return Value;
        //}
        //public double TeleprospectingSourced_Tele_TotalMarketingQualifiedLeadsMQL() //K16
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TGL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double TeleprospectingSourced_Tele_TQL_ConversionGate(bool isBaseline) //N17
        //{
        //    if (isBaseline)
        //    {
        //        ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        TTGLSALC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //        CurrentBaseline = TTGLSALC;
        //        BIC = 85;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = TTGLSALC + Improvement;
        //    }

        //    return ConversionGate;
        //}

        //public double TeleprospectingSourced_Sales_SAL() //K18
        //{
        //    double TQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_TQL).Select(m => m.ProspectingSourced).FirstOrDefault()) / 100;
        //    double TGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TGL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = TQL_ConversionGate * TGL;
        //    return Value;
        //}
        //public double TeleprospectingSourced_Sales_SAL_ConversionGate(bool isBoolean) //N20
        //{
        //    if (isBoolean)
        //    {
        //        ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        TSALSQLC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //        CurrentBaseline = TSALSQLC;
        //        BIC = 85;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = TSALSQLC + Improvement;
        //    }

        //    return ConversionGate;
        //}
        //public double TeleprospectingSourced_Sales_SQL() //K21
        //{
        //    double SAL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SAL).Select(m => m.ProspectingSourced).FirstOrDefault()) / 100;
        //    double SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = SAL_ConversionGate * SAL;
        //    return Value;
        //}
        //public double TeleprospectingSourced_Sales_SQL_ConversionGate(bool isBaseline) //N22
        //{
        //    if (isBaseline)
        //    {
        //        ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        TSQLCWC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //        CurrentBaseline = TSQLCWC;
        //        BIC = 29.1;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = TSQLCWC + Improvement;
        //    }

        //    return ConversionGate;
        //}
        //public double TeleprospectingSourced_Sales_CW() //N23
        //{
        //    double SQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SQL).Select(m => m.ProspectingSourced).FirstOrDefault()) / 100;
        //    double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = SQL_ConversionGate * SQL;
        //    return Value;
        //}

        //public double TeleprospectingSourced_Average_Deal_Size(bool isBaseline) //N25
        //{
        //    if (isBaseline)
        //    {
        //        AverageDealSize = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.AverageDealSize).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        TDSZ = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.AverageDealSize).FirstOrDefault();
        //        CurrentBaseline = TDSZ;
        //        BIC = 10;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        AverageDealSize = TDSZ * (1 + Improvement);
        //    }

        //    return AverageDealSize;
        //}
        //public double TeleprospectingSourced_Expected_Revenue() //K26
        //{
        //    double AverageDealsSize = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AverageDealsSize).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = AverageDealsSize * CW;
        //    return Value;
        //}

        //#endregion

        //#region TeleprospectingStageDays

        //public double TeleprospectingStageDays_Tele_TotalMarketingQualifiedLeadsMQL() //Q16
        //{
        //    Ifactor = IFactor();
        //    TTQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == TQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    CurrentBaseline = TTQLL;
        //    BIC = 2;
        //    CurrentGap = BIC - CurrentBaseline;
        //    if (CurrentGap > 0)
        //    {
        //        Improvement = CurrentGap * Ifactor;
        //    }
        //    else
        //    {
        //        Improvement = 0;
        //    }
        //    Value = TTQLL - Improvement;
        //    return Value;
        //}

        //public double TeleprospectingStageDays_Sales_SAL() //Q18
        //{
        //    Ifactor = IFactor();
        //    TTQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    CurrentBaseline = TTQLL;
        //    BIC = 5;
        //    CurrentGap = BIC - CurrentBaseline;
        //    if (CurrentGap > 0)
        //    {
        //        Improvement = CurrentGap * Ifactor;
        //    }
        //    else
        //    {
        //        Improvement = 0;
        //    }
        //    Tele_SAL_Stage = TTQLL - Improvement;
        //    return Tele_SAL_Stage;
        //}
        //public double TeleprospectingStageDays_Sales_SQL() //Q21
        //{
        //    Ifactor = IFactor();
        //    TSQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Teleprospecting).Select(m => m.Value).FirstOrDefault();
        //    CurrentBaseline = TSQLL;
        //    BIC = 65;
        //    CurrentGap = BIC - CurrentBaseline;
        //    if (CurrentGap > 0)
        //    {
        //        Improvement = CurrentGap * Ifactor;
        //    }
        //    else
        //    {
        //        Improvement = 0;
        //    }
        //    Tele_SQL_Stage = TSQLL - Improvement;
        //    return Tele_SQL_Stage;
        //}
        //#endregion

        //#endregion

        //#region Sales

        //#region SalesSourced

        //public double SalesSourced_Sales_SGL(bool isBaseline) //V19
        //{
        //    if (isBaseline)
        //    {
        //        ConversionGate = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Sales).Select(m => m.ExpectedLeadCount).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        ISGL = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Sales).Select(m => m.ExpectedLeadCount).FirstOrDefault();
        //        CurrentBaseline = ISGL;
        //        BIC = 5;
        //        CurrentGap = BIC;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = ISGL * (1 + Improvement);
        //    }

        //    return ConversionGate;
        //}
        //public double SalesSourced_Sales_SAL_ConversionGate(bool isBaseline) //V20
        //{
        //    if (isBaseline)
        //    {
        //        ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //    }
        //    else
        //    {

        //        Ifactor = IFactor();
        //        SSGLSQLC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //        CurrentBaseline = SSGLSQLC;
        //        BIC = 61.7;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = SSGLSQLC + Improvement;
        //    }

        //    return ConversionGate;
        //}
        //public double SalesSourced_Sales_SQL() //V21
        //{
        //    double SGL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SAL).Select(m => m.SalesSourced).FirstOrDefault()) / 100;
        //    double SGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.SalesSourced).FirstOrDefault());
        //    Value = SGL * SGL_ConversionGate;
        //    return Value;
        //}
        //public double SalesSourced_Sales_SQL_ConversionGate(bool isBaseline) //V22
        //{
        //    if (isBaseline)
        //    {
        //        ConversionGate = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        SSQLCWC = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == CR && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //        CurrentBaseline = SSQLCWC;
        //        BIC = 29.1;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        ConversionGate = SSQLCWC + Improvement;

        //    }
        //    return ConversionGate;
        //}
        //public double SalesSourced_Sales_CW() //V23
        //{
        //    double SQL_ConversionGate = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SQL).Select(m => m.SalesSourced).FirstOrDefault()) / 100;
        //    double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.SalesSourced).FirstOrDefault());
        //    Value = SQL * SQL_ConversionGate;
        //    return Value;
        //}

        //public double SalesSourced_Average_Deal_Size(bool isBaseline) //V25
        //{
        //    if (isBaseline)
        //    {
        //        AverageDealSize = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Sales).Select(m => m.AverageDealSize).FirstOrDefault();
        //    }
        //    else
        //    {
        //        Ifactor = IFactor();
        //        SDSZ = db.Model_Funnel.Where(m => m.ModelId == ModelId && m.Funnel.Title.ToLower() == Sales).Select(m => m.AverageDealSize).FirstOrDefault();
        //        CurrentBaseline = SDSZ;
        //        BIC = 10;
        //        CurrentGap = BIC - CurrentBaseline;
        //        if (CurrentGap > 0)
        //        {
        //            Improvement = CurrentGap * Ifactor;
        //        }
        //        else
        //        {
        //            Improvement = 0;
        //        }
        //        AverageDealSize = SDSZ * (1 + Improvement);

        //    }
        //    return AverageDealSize;
        //}
        //public double SalesSourced_Expected_Revenue() //S26
        //{
        //    double AverageDealsSize = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_AverageDealsSize).Select(m => m.SalesSourced).FirstOrDefault());
        //    double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.SalesSourced).FirstOrDefault());
        //    Value = AverageDealsSize * CW;
        //    return Value;
        //}

        //#endregion

        //#region SalesStageDays

        //public double SalesStageDays_Sales_SGL() //Y19
        //{
        //    Ifactor = IFactor();
        //    SSALL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SAL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //    CurrentBaseline = SSALL;
        //    BIC = 5;
        //    CurrentGap = BIC - CurrentBaseline;
        //    if (CurrentGap > 0)
        //    {
        //        Improvement = CurrentGap * Ifactor;
        //    }
        //    else
        //    {
        //        Improvement = 0;
        //    }
        //    Sales_SAL_Stage = SSALL - Improvement;
        //    return Sales_SAL_Stage;
        //}
        //public double SalesStageDays_Sales_SQL() //Y21
        //{
        //    Ifactor = IFactor();
        //    SSQLL = db.Model_Funnel_Stage.Where(m => m.Model_Funnel.ModelId == ModelId && m.StageType.ToLower() == SV && m.Stage.Code.ToLower() == SQL && m.Model_Funnel.Funnel.Title.ToLower() == Sales).Select(m => m.Value).FirstOrDefault();
        //    CurrentBaseline = SSQLL;
        //    BIC = 65;
        //    CurrentGap = BIC - CurrentBaseline;
        //    if (CurrentGap > 0)
        //    {
        //        Improvement = CurrentGap * Ifactor;
        //    }
        //    else
        //    {
        //        Improvement = 0;
        //    }
        //    Sales_SQL_Stage = SSQLL - Improvement;
        //    return Sales_SQL_Stage;
        //}

        //public double SalesStageDays_BlendedFull() //T24
        //{
        //    double SGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.SalesDays).FirstOrDefault());
        //    double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.SalesDays).FirstOrDefault());
        //    Value = SQL + SGL;
        //    return Value;
        //}

        //#endregion

        //#endregion

        #region Blended

        #region BlendedTotal

        public double BlendedTotal_Mkt_DatabaseSize() //AA4
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_DatabaseSize).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_SUS_ConversionGate() //AA5
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_SUS).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_OutboundGeneratedInquiries() //AA6
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_InboundInquiries() //AA7
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_TotalInquiries() //AA8
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_TotalInquiries).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_INQ_ConversionGate() //AA9
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_INQ).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_AQL() //AA10
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AQL).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }
        public double BlendedTotal_Mkt_AQL_ConversionGate() //AA11
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_AQL).Select(m => m.MarketingSourced).FirstOrDefault());
            return Value;
        }

        //public double BlendedTotal_Tele_TAL() //AA12
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TAL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double BlendedTotal_Tele_TAL_ConversionGate() //AA13
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ConversionGate_TAL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double BlendedTotal_Tele_TQL() //AA14
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TQL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double BlendedTotal_Tele_TGL() //AA15
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TGL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL() //AA16
        //{
        //    double TQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TQL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    double TGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TGL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = TQL + TGL;
        //    return Value;
        //}
        //public double BlendedTotal_Tele_TQL_ConversionGate() //AA17
        //{
        //    double TotalMarketingQualifiedLeadsMQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.BlendedTotal).FirstOrDefault());
        //    if (TotalMarketingQualifiedLeadsMQL == 0)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        double SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.BlendedTotal).FirstOrDefault());
        //        Value = SAL / TotalMarketingQualifiedLeadsMQL * 100;
        //        return Value;
        //    }
        //}

        //public double BlendedTotal_Sales_SAL() //AA18
        //{
        //    double MarketingSourced_Sales_SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    double TeleprospectingSourced_Sales_SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    Value = MarketingSourced_Sales_SAL + TeleprospectingSourced_Sales_SAL;
        //    return Value;
        //}
        //public double BlendedTotal_Sales_SGL() //AA19
        //{
        //    Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.SalesSourced).FirstOrDefault());
        //    return Value;
        //}
        //public double BlendedTotal_Sales_SAL_ConversionGate() //AA20
        //{
        //    double SGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.BlendedTotal).FirstOrDefault());
        //    double SAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.BlendedTotal).FirstOrDefault());
        //    double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.BlendedTotal).FirstOrDefault());
        //    if (SAL + SGL == 0)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        Value = SQL / (SAL + SGL) * 100;
        //        return Value;
        //    }
        //}
        //public double BlendedTotal_Sales_SQL() //AA21
        //{
        //    double Mkt_SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.MarketingSourced).FirstOrDefault());
        //    double Tele_SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    double Sales_SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.SalesSourced).FirstOrDefault());
        //    Value = Mkt_SQL + Tele_SQL + Sales_SQL;
        //    return Value;
        //}
        //public double BlendedTotal_Sales_SQL_ConversionGate() //AA22
        //{
        //    double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.BlendedTotal).FirstOrDefault());
        //    if (SQL == 0)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.BlendedTotal).FirstOrDefault());
        //        Value = (CW / SQL) * 100;
        //        return Value;
        //    }
        //}
        //public double BlendedTotal_Sales_CW() //AA23
        //{
        //    double Mkt_CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.MarketingSourced).FirstOrDefault());
        //    double Tele_CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.ProspectingSourced).FirstOrDefault());
        //    double Sales_CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.SalesSourced).FirstOrDefault());
        //    Value = Mkt_CW + Tele_CW + Sales_CW;
        //    return Value;
        //}
        public double BlendedTotal_BlendedFull() //AA24
        {
            double OutboundGeneratedInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.BlendedTotal).FirstOrDefault());
            double InboundInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.BlendedTotal).FirstOrDefault());
            double TGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TGL).Select(m => m.BlendedTotal).FirstOrDefault());
            double SGL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.BlendedTotal).FirstOrDefault());
            double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.BlendedTotal).FirstOrDefault());

            if ((OutboundGeneratedInquiries + InboundInquiries + TGL + SGL + CW) == 0)
            {
                return 0;
            }
            else
            {
                Value = CW / (OutboundGeneratedInquiries + InboundInquiries + TGL + SGL);
                return Value;
            }
        }
        public double BlendedTotal_AverageDealSize() //AA25
        {
            double CW = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ClosedWon).Select(m => m.BlendedTotal).FirstOrDefault());
            if (CW == 0)
            {
                return 0;
            }
            else
            {
                double Expected_Revenue = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ExpectedRevenue).Select(m => m.BlendedTotal).FirstOrDefault());
                Value = Expected_Revenue / CW;
                return Value;
            }
        }
        public double BlendedTotal_Expected_Revenue() //AA26
        {
            double Mkt_ER = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == ExpectedRevenue).Select(m => m.MarketingSourced).FirstOrDefault());
            double Tele_ER = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == ExpectedRevenue).Select(m => m.ProspectingSourced).FirstOrDefault());
            double Sales_ER = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == ExpectedRevenue).Select(m => m.SalesSourced).FirstOrDefault());
            Value = Mkt_ER + Tele_ER + Sales_ER;
            return Value;
        }

        #endregion

        #region BlendedTotalDays

        public double BlendedTotalDays_Mkt_OutboundGeneratedInquiries() //AB6
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_OutboundGeneratedInquiries).Select(m => m.MarketingDays).FirstOrDefault());
            return Value;
        }
        public double BlendedTotalDays_Mkt_InboundInquiries() //AB7
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_InboundInquiries).Select(m => m.MarketingDays).FirstOrDefault());
            return Value;
        }
        public double BlendedTotalDays_Mkt_TotalInquiries() //AB8
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_TotalInquiries).Select(m => m.MarketingDays).FirstOrDefault());
            return Value;
        }
        public double BlendedTotalDays_Mkt_AQL() //AB10
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AQL).Select(m => m.MarketingDays).FirstOrDefault());
            return Value;
        }

        public double BlendedTotalDays_Tele_TAL() //AB12
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TAL).Select(m => m.MarketingDays).FirstOrDefault());
            return Value;
        }
        public double BlendedTotalDays_Tele_TotalMarketingQualifiedLeadsMQL(double BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL = 0) //AB16
        {
            if (BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL == 0)
            {
                return 0;
            }
            else
            {
                double MQL_MS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.MarketingSourced).FirstOrDefault());
                double MQL_MD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.MarketingDays).FirstOrDefault());
                double MQL_PS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.ProspectingSourced).FirstOrDefault());
                double MQL_PD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.ProspectingDays).FirstOrDefault());
                Value = ((MQL_MS / BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL) * MQL_MD + (MQL_PS / BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL) * MQL_PD);
                return Value;
            }
        }

        public double BlendedTotalDays_Sales_SAL(double BlendedTotal_Sales_SAL = 0) //AB18
        {
            if (BlendedTotal_Sales_SAL == 0)
            {
                return 0;
            }
            else
            {
                double SAL_MS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.MarketingSourced).FirstOrDefault());
                double SAL_MD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.MarketingDays).FirstOrDefault());
                double SAL_PS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.ProspectingSourced).FirstOrDefault());
                double SAL_PD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.ProspectingDays).FirstOrDefault());
                Value = ((SAL_MS / BlendedTotal_Sales_SAL) * SAL_MD + (SAL_PS / BlendedTotal_Sales_SAL) * SAL_PD);
                return Value;
            }
        }
        public double BlendedTotalDays_Sales_SGL() //AB19
        {
            Value = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.SalesDays).FirstOrDefault());
            return Value;
        }
        public double BlendedTotalDays_Sales_SQL(double BlendedTotal_Sales_SQL = 0) //AB21
        {
            if (BlendedTotal_Sales_SQL == 0)
            {
                return 0;
            }
            else
            {
                double SQL_MS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.MarketingSourced).FirstOrDefault());
                double SQL_MD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.MarketingDays).FirstOrDefault());
                double SQL_PS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.ProspectingSourced).FirstOrDefault());
                double SQL_PD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.ProspectingDays).FirstOrDefault());
                double SQL_SS = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.SalesSourced).FirstOrDefault());
                double SQL_SD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.SalesDays).FirstOrDefault());
                Value = ((SQL_MS / BlendedTotal_Sales_SQL) * SQL_MD + (SQL_PS / BlendedTotal_Sales_SQL) * SQL_PD + (SQL_SS / BlendedTotal_Sales_SQL) * SQL_SD);
                return Value;
            }
        }

        public double BlendedTotalDays_BlendedFull() //AB24
        {
            double TotalInquiries = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_TotalInquiries).Select(m => m.BlendedDays).FirstOrDefault());
            double AQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Marketing && m.Funnel_Field.Field.Title.ToLower() == FF_AQL).Select(m => m.BlendedDays).FirstOrDefault());
            double TAL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_TAL).Select(m => m.BlendedDays).FirstOrDefault());
            double MQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Teleprospecting && m.Funnel_Field.Field.Title.ToLower() == FF_MQL).Select(m => m.BlendedDays).FirstOrDefault());
            double SAL_BT = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.BlendedTotal).FirstOrDefault());
            double SGL_BT = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.BlendedTotal).FirstOrDefault());
            double SAL_BD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SAL).Select(m => m.BlendedDays).FirstOrDefault());
            double SGL_BD = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SGL).Select(m => m.BlendedDays).FirstOrDefault());
            double SQL = Convert.ToDouble(db.ModelReviews.Where(m => m.Model_Funnel.ModelId == ModelId && m.Funnel_Field.Funnel.Title.ToLower() == Sales && m.Funnel_Field.Field.Title.ToLower() == FF_SQL).Select(m => m.BlendedDays).FirstOrDefault());
            if ((SAL_BT + SGL_BT) == 0)
            {
                Value = 0;
            }
            else
            {
                Value = (SGL_BT / (SGL_BT + SAL_BT) * SAL_BD) + (SGL_BT / (SGL_BT + SAL_BT) * SGL_BD);
            }
            return TotalInquiries + AQL + TAL + MQL + Value + SQL;
        }

        #endregion

        #endregion
    }
}