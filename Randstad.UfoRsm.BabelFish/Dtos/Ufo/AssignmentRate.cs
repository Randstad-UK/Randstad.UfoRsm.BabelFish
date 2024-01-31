using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;
//using Randstad.UfoRsm.BabelFish.Dtos.Sti;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Template.Extensions;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class AssignmentRate
    {
        public string FeeRef { get; set; }
        public string FeeName { get; set; }

        public string StartDate { get; set; }

        public string ExpenseType { get; set; }

        public string OvertimeType { get; set; }
        public bool NonChargeableToClient { get; set; }

        public string PayUnit { get; set; }
        public string SequenceNumber { get; set; }
        public decimal? PayRateCurrency { get; set; }
        public decimal? ChargeRateCurrency { get; set; }
        public Decimal? PostParityPayRateCurrency { get; set; }
        public Decimal? PostParityChargeRateCurrency { get; set; }

        public string RateType { get; set; }

        public Assignment Assignment { get; set; }

        public RSM.Rate MapRate(Dictionary<string, string> rateCodes, Assignment assignment, out RSM.Rate postRate)
        {
            Assignment = assignment;
            return MapRate(rateCodes, out postRate);
        }

        public RSM.Rate MapRate(Dictionary<string, string> rateCodes, out RSM.Rate postRate)
        {
            postRate = null;

            var rate = new RSM.Rate();
            rate.awrSpecified = true;
            rate.awrSpecified = false;
            rate.chargeSpecified = false;
            rate.commentsEnabledSpecified = false;
            rate.effectiveFromSpecified = true;

            if (!string.IsNullOrEmpty(StartDate))
            {
                rate.effectiveFrom = DateTime.Parse(StartDate);
            }

            rate.name = FeeName;
            rate.payableSpecified = false;
            rate.ExternalAssignmentRef = Assignment.AssignmentRef;
            rate.frontendRef = FeeRef;
            rate.backendRef = Assignment.AssignmentRef;
            SetRateType(rate, rateCodes, false);


            postRate = new RSM.Rate();
            postRate.awrSpecified = true;
            postRate.awr = true;
            postRate.chargeSpecified = false;
            postRate.commentsEnabledSpecified = false;
            postRate.effectiveFromSpecified = true;

            if (!string.IsNullOrEmpty(StartDate))
            {
                postRate.effectiveFrom = DateTime.Parse(StartDate);
            }

            postRate.name = FeeName;
            postRate.payableSpecified = false;
            postRate.ExternalAssignmentRef = Assignment.AssignmentRef;
            postRate.frontendRef = FeeRef + "-AWR";
            postRate.backendRef = Assignment.AssignmentRef;
            SetRateType(postRate, rateCodes, true);


            return rate;
        }


        private void SetRateType(RSM.Rate rate, Dictionary<string, string> rateCodes, bool isPostParity)
        {
            rate.proRataSpecified = false;
            rate.periodDurationSpecified = false;
            rate.secondaryPaySpecified = false;
            rate.periodDurationSpecified = false;
            rate.selectableByWorkersSpecified = true;
            rate.selectableByWorkers = false;
            rate.taxableSpecified = false;
            rate.timePattern = "default";

            if (PayRateCurrency == null)
            {
                PayRateCurrency = 0;
            }

            if (ChargeRateCurrency == null)
            {
                ChargeRateCurrency = 0;
            }

            switch (RateType)
            {
                case "Basic Rate":
                    {
                        //Concat  the mapname to be Basic Hours or Basic Days
                        var mapName = "Basic ";

                        rate.timesheetFields = "Hours";

                        if (PayUnit == "Hourly")
                        {
                            mapName += "Hours";

                            //for multistudent support need to pick rate that does not validate minimum wage in RSM
                            if (Assignment.MultiStudentSupport)
                            {
                                mapName = "Basic Hours non NMW";
                            }

                            rate.periodDuration = 60;
                            rate.periodDurationSpecified = true;

                            rate.timesheetFields = "START_FINISH_BREAK";
                            rate.period = "Hourly";
                        }

                        if (PayUnit == "Daily")
                        {
                            mapName += "Days";
                            rate.periodDuration = 480;

                            rate.timesheetFields = "DECIMAL";
                            rate.period = "Fixed";

                        }

                        rate.payElementCode = rateCodes[mapName];

                        if (PayRateCurrency != null && !isPostParity)
                        {
                            rate.pay = (decimal)PayRateCurrency;
                            rate.paySpecified = true;
                        }

                        if (ChargeRateCurrency != null && !isPostParity)
                        {
                            rate.charge = (decimal)ChargeRateCurrency;
                            rate.chargeSpecified = true;
                        }

                        //for post parity if the postparity payrate is set use it, otherwise use payrate
                        if (PayRateCurrency != null && isPostParity)
                        {
                            if (PostParityPayRateCurrency != null)
                            {
                                rate.pay = (decimal)PostParityPayRateCurrency;
                            }
                            else
                            {
                                rate.pay = (decimal)PayRateCurrency;
                            }

                            rate.paySpecified = true;
                        }

                        //for post parity if the postparity chargerate is set use it, otherwise use chargerate
                        if (ChargeRateCurrency != null && isPostParity)
                        {

                            if (PostParityChargeRateCurrency != null)
                            {
                                rate.charge = (decimal)PostParityChargeRateCurrency;
                            }
                            else
                            {
                                rate.charge = (decimal)ChargeRateCurrency;
                            }

                            rate.chargeSpecified = true;
                        }

                        rate.name = mapName;

                        break;
                    }
                case "Other Rate":
                    {
                        rate.payElementCode = rateCodes[OvertimeType];
                        rate.periodDurationSpecified = true;

                        rate.timesheetFields = "HOURS";

                        if (PayRateCurrency != null && !isPostParity)
                        {
                            rate.pay = (decimal)PayRateCurrency;
                            rate.paySpecified = true;
                        }

                        if (ChargeRateCurrency != null && !isPostParity)
                        {
                            rate.charge = (decimal)ChargeRateCurrency;
                            rate.chargeSpecified = true;
                        }

                        //for post parity if the postparity payrate is set use it, otherwise use payrate
                        if (PayRateCurrency != null && isPostParity)
                        {
                            if (PostParityPayRateCurrency != null)
                            {
                                rate.pay = (decimal)PostParityPayRateCurrency;
                            }
                            else
                            {
                                rate.pay = (decimal)PayRateCurrency;
                            }

                            rate.paySpecified = true;
                        }

                        //for post parity if the postparity chargerate is set use it, otherwise use charge rate
                        if (ChargeRateCurrency != null && isPostParity)
                        {

                            if (PostParityChargeRateCurrency != null)
                            {
                                rate.charge = (decimal)PostParityChargeRateCurrency;
                            }
                            else
                            {
                                rate.charge = (decimal)ChargeRateCurrency;
                            }

                            rate.chargeSpecified = true;
                        }

                        if (PayUnit == "Hourly")
                        {
                            rate.periodDuration = 60;
                            rate.periodDurationSpecified = true;

                            rate.timesheetFields = "START_FINISH_BREAK";
                            rate.period = "Hourly";
                        }

                        if (PayUnit == "Daily")
                        {
                            rate.periodDuration = 480;
                            rate.timesheetFields = "DECIMAL";
                            rate.period = "Fixed";
                        }

                        rate.name = FeeName;
                        break;
                    }
                case "Expense Rate":
                    {
                        rate.payElementCode = rateCodes[ExpenseType];
                        rate.period = "FIXED";

                        rate.timesheetFields = "DECIMAL";
                        if (PayRateCurrency != null)
                        {
                            rate.pay = (decimal)PayRateCurrency;
                            rate.paySpecified = true;

                        }

                        if (ChargeRateCurrency != null)
                        {
                            rate.charge = (decimal)ChargeRateCurrency;
                            rate.chargeSpecified = true;
                        }

                        rate.name = FeeName;

                        if (NonChargeableToClient)
                        {
                            rate.payElementCode = rate.payElementCode + "NC";
                            rate.name = rate.name + " NC";
                        }

                        break;
                    }

            }

            if (!string.IsNullOrEmpty(Assignment.SendRatesFormat))
            {
                switch (Assignment.SendRatesFormat)
                {
                    case "Send Hourly Rates as DECIMAL":
                        {
                            rate.timesheetFields = "DECIMAL";
                            rate.period = "Fixed";
                            break;
                        }
                    case "Send Hourly Rates as HOURS":
                        {
                            rate.timesheetFields = "HOURS";
                            rate.period = "Hourly";
                            break;
                        }
                }
            }
        }

    }
}
