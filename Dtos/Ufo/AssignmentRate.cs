using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
//using Randstad.UfoRsm.BabelFish.Dtos.Sti;
using Randstad.UfoRsm.BabelFish.Helpers;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class AssignmentRate
    {
        public string Label { get; set; }
        public string FeeRef { get; set; }
        public string FeeName { get; set; }
        public bool? Active { get; set; }
        public double? ActiveHourlyRate { get; set; }
        public double? AdditionalLabourCost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ExpenseType { get; set; }
        public bool? IsModifiedOnAssignment { get; set; }
        public double? Margin { get; set; }
        public string OvertimeType { get; set; }
        public bool? IsPayRise { get; set; }
        public string PayUnit { get; set; }
        public string SequenceNumber { get; set; }

        public decimal? PayRateCurrency { get; set; }
        public decimal? ChargeRateCurrency { get; set; }

        public bool? ReceiptRequired { get; set; }
        public string RateType { get; set; }
        public double? StandbyFee { get; set; }
        public string Status { get; set; }
        public Assignment Assignment { get; set; }

        public RSM.Rate MapRate(Dictionary<string, string> rateCodes, Assignment assignment)
        {
            Assignment = assignment;
            return MapRate(rateCodes);
        }

        public RSM.Rate MapRate(Dictionary<string, string> rateCodes)
        {
            var rate = new RSM.Rate();
            rate.awrSpecified = true;
            rate.awrSpecified = false;
            rate.chargeSpecified = false;
            rate.commentsEnabledSpecified = false;
            rate.effectiveFromSpecified = true;
            rate.effectiveFrom = StartDate;
            rate.name = FeeName;
            rate.pay = PayRateCurrency;
            rate.payableSpecified = false;

            SetRateType(rate, rateCodes);

            return rate;
        }


        
        
        private void SetRateType(RSM.Rate rate, Dictionary<string, string> rateCodes)
        {
            rate.proRataSpecified = false;
            rate.secondaryPaySpecified = false;
            rate.selectableByWorkersSpecified = true;
            rate.selectableByWorkers = false;
            rate.taxableSpecified = false;
            rate.timePattern = "default";

            rate.timesheetFields = @"start = ""false"" finish = ""false"" break= ""false"" hours = ""false"" dayDecimal = ""true"" isDay = ""false"" comment = ""false""";
            switch (RateType)
            {
                case "Basic Rate":
                {
                    //Concat  the mapname to be Basic Hours or Basic Days
                    var mapName = "Basic ";

                    
                    if (PayUnit == "Hourly")
                    {
                        mapName += "Hours";
                        rate.periodDuration = 60;
                        rate.timesheetFields = @"start = ""false"" finish = ""false"" break= ""false"" hours = ""true"" dayDecimal = ""false"" isDay = ""false"" comment = ""false""";
                    }

                    if (PayUnit == "Daily")
                    {
                        mapName += "Days";
                        rate.periodDuration = 480;
                    }


                    rate.payElementCode = rateCodes[mapName];
                    rate.period = mapName;

                    if (PayRateCurrency != null)
                        rate.pay = (decimal) PayRateCurrency;

                    if (ChargeRateCurrency != null)
                        rate.charge = (decimal) ChargeRateCurrency;



                    break;
                }
                case "Other Rate":
                {
                    rate.payElementCode = rateCodes[OvertimeType];

                    if (PayRateCurrency != null)
                        rate.pay = (decimal)PayRateCurrency;

                    if (ChargeRateCurrency != null)
                        rate.charge = (decimal)ChargeRateCurrency;
                    break;
                }

            }
        }

    }
}
