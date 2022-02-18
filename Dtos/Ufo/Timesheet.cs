using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.Logging.Core;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Template.Extensions;
using Randstad.UfoRsm.BabelFish.Translators;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Timesheet : ObjectBase
    {
        public string TimesheetId { get; set; }

        public string AssignmentRef { get; set; }
        public string HolidayPay { get; set; }
        public string ClientRef { get; set; }
        public string ClientName { get; set; }
        public string LtdCompany { get; set; }
        public string Umbrella { get; set; }
        public string Outsourced { get; set; }
        public string PaymentType { get; set; }

        public string CandidateName{get; set;}
        public string JobTitle { get; set; }

        public string ClientApprover { get; set; }

        public string HleRef { get; set; }
        public string InvoiceAddressId { get; set; }

        public string PayrollRef { get; set; }
        public string PoNumber { get; set; }
        public Owner Owner{ get; set; }

        public string TimesheetRef { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public string ExternalTimesheetId { get; set; }


        public DateTime CreatedDate { get; set; }
        public Team OpCo { get; set; }
        public string Contact{ get; set; }
        
        public string Description { get; set; }
        
        public Address WorkAddress { get; set; }
        public string CostCentre { get; set; }
        public decimal? TotalDays { get; set; }
        public List<ConsultantSplit> ConsultantSplits { get; set; }


        public List<TimesheetLine> TimesheetLines { get; set; }
        public List<Expense> Expenses { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        private string _OriginalTimesheetRef { get; set; }

        private List<TimesheetLine> GetBasicHours(out TimesheetLine consolidatedBasicHours)
        {
            try
            {
                consolidatedBasicHours = null;
                if (TimesheetLines == null) return null;

                var basicHours = TimesheetLines.Where(x =>
                    x.HoursType.ToLower() == "normal time" && (x.Rate.RateType.ToLower() == "basic rate") &&
                    x.Rate.PayUnit.ToLower() == "hourly").ToList();

                if (!basicHours.Any())
                {
                    consolidatedBasicHours = null;
                    return null;
                }

                consolidatedBasicHours = GetConsolidatedLine(basicHours);
                return basicHours;
            }
            catch (Exception exp)
            {
                throw new Exception($"{_OriginalTimesheetRef} Problem mapping Basic Hours. Hours Type should be normal time, Rate needs to be basic Rate, Rate pay unit should be hourly "+exp.Message, exp);
            }
        }


        private TimesheetLine GetConsolidatedLine(List<TimesheetLine> lines)
        {
            var consolidatedLine = new TimesheetLine();
            consolidatedLine = JsonConvert.DeserializeObject<TimesheetLine>(JsonConvert.SerializeObject(lines[0]));
            
            
            if (consolidatedLine.TotalDays == null && consolidatedLine.DaysReported==null)
                consolidatedLine.TotalDays = 0;
            else
                consolidatedLine.TotalDays = lines[0].DaysReported;

            lines[0].StartTime = new DateTime(lines[0].StartDateTime.Value.Year,
                                            lines[0].StartDateTime.Value.Month,
                                            lines[0].StartDateTime.Value.Day,
                                            lines[0].StartTime.Value.Hour,
                                            lines[0].StartTime.Value.Minute,
                                            lines[0].StartTime.Value.Second);

            lines[0].EndTime = new DateTime(lines[0].EndDateTime.Value.Year,
                                            lines[0].EndDateTime.Value.Month,
                                            lines[0].EndDateTime.Value.Day,
                                            lines[0].EndTime.Value.Hour,
                                            lines[0].EndTime.Value.Minute,
                                            lines[0].EndTime.Value.Second);

            for (var i = 1; i<lines.Count; i++)
            {
                lines[i].StartTime = new DateTime(lines[i].StartDateTime.Value.Year,
                lines[i].StartDateTime.Value.Month,
                lines[i].StartDateTime.Value.Day,
                lines[i].StartTime.Value.Hour,
                lines[i].StartTime.Value.Minute,
                lines[i].StartTime.Value.Second);

                lines[i].EndTime = new DateTime(lines[i].EndDateTime.Value.Year,
                lines[i].EndDateTime.Value.Month,
                lines[i].EndDateTime.Value.Day,
                lines[i].EndTime.Value.Hour,
                lines[i].EndTime.Value.Minute,
                lines[i].EndTime.Value.Second);

                if (lines[i].TotalHours!=null)
                    consolidatedLine.TotalHours = consolidatedLine.TotalHours + lines[i].TotalHours;

                if(lines[i].BreakTimeMinutes!=null)
                    consolidatedLine.BreakTimeMinutes = consolidatedLine.BreakTimeMinutes + lines[i].BreakTimeMinutes;

                if(lines[i].DaysReported!=null)
                    consolidatedLine.TotalDays = consolidatedLine.TotalDays + lines[i].DaysReported;
            }

            return consolidatedLine;
        }

        private List<TimesheetLine> GetBasicDays(out TimesheetLine consolidatedBasicDays)
        {
            try
            {
                consolidatedBasicDays = null;
                if (TimesheetLines == null) return null;

                var basicDays = TimesheetLines.Where(x =>
                    x.HoursType.ToLower() == "normal time" && x.Rate.RateType.ToLower() == "basic rate" &&
                    x.Rate.PayUnit.ToLower() == "daily").OrderByDescending(y=>y.Rate.EndDate).ToList();

                if (!basicDays.Any())
                {
                    consolidatedBasicDays = null;
                    return null;
                }

                consolidatedBasicDays = GetConsolidatedLine(basicDays);


                return basicDays;
            }
            catch (Exception exp)
            {
                throw new Exception($"{_OriginalTimesheetRef} Problem mapping Basic Days. Hours Type should be normal time, Rate needs to be basic Rate, Rate pay unit should be daily " + exp.Message, exp);
            }
        }

        private List<IGrouping<string, Expense>> GetConsolidatedExpenses(out List<Expense> consolidatedExpenses)
        {
            consolidatedExpenses = null;

            if (Expenses == null) return null;

            var expenses = Expenses.GroupBy(x => x.ExpenseType).ToList();

            if (!expenses.Any())
            {
                consolidatedExpenses = null;
                return null;
            }

            consolidatedExpenses = new List<Expense>();
            foreach (var a in expenses)
            {
                var expenseLines = a.AsEnumerable().ToList();
                var expenseConsolidated = JsonConvert.DeserializeObject<Expense>(JsonConvert.SerializeObject(expenseLines.FirstOrDefault()));

                for (var i = 1; i < expenseLines.Count(); i++)
                {
                    if (expenseLines[i].ExpenseType.ToLower() == "mileage")
                    {
                        expenseConsolidated.Quantity = expenseConsolidated.Quantity + expenseLines[i].Quantity;
                    }
                    else
                    {
                        expenseConsolidated.Quantity = 1;
                        expenseConsolidated.Amount = expenseConsolidated.Amount + expenseLines[i].Amount;
                    }
                }

                consolidatedExpenses.Add(expenseConsolidated);
            }

            return expenses;

        }

        private List<IGrouping<string, TimesheetLine>> GetOverTimeHours(out List<TimesheetLine> consolidatedLines)
        {
            try
            {
                consolidatedLines = null;
                if (TimesheetLines == null) return null;

                var lines = TimesheetLines.Where(x => x.Rate.RateType.ToLower() == "other rate" &&
                                                      x.Rate.PayUnit.ToLower() == "hourly").OrderByDescending(y=>y.EndDateTime).GroupBy(x => x.HoursType)
                    .ToList();

                if (!lines.Any())
                {
                    consolidatedLines = null;
                    return null;
                }

                consolidatedLines = new List<TimesheetLine>();
                foreach (var a in lines)
                {
                    var overTimeLines = a.AsEnumerable().ToList();
                    var otConsolidated =
                        JsonConvert.DeserializeObject<TimesheetLine>(
                            JsonConvert.SerializeObject(overTimeLines.FirstOrDefault()));

                    overTimeLines[0].StartTime = new DateTime(overTimeLines[0].StartDateTime.Value.Year,
                        overTimeLines[0].StartDateTime.Value.Month,
                        overTimeLines[0].StartDateTime.Value.Day,
                        overTimeLines[0].StartTime.Value.Hour,
                        overTimeLines[0].StartTime.Value.Minute,
                        overTimeLines[0].StartTime.Value.Second);

                    overTimeLines[0].EndTime = new DateTime(overTimeLines[0].EndDateTime.Value.Year,
                        overTimeLines[0].EndDateTime.Value.Month,
                        overTimeLines[0].EndDateTime.Value.Day,
                        overTimeLines[0].EndTime.Value.Hour,
                        overTimeLines[0].EndTime.Value.Minute,
                        overTimeLines[0].EndTime.Value.Second);

                    for (var i = 1; i < overTimeLines.Count(); i++)
                    {
                        overTimeLines[i].StartTime = new DateTime(overTimeLines[i].StartDateTime.Value.Year,
                            overTimeLines[i].StartDateTime.Value.Month,
                            overTimeLines[i].StartDateTime.Value.Day,
                            overTimeLines[i].StartTime.Value.Hour,
                            overTimeLines[i].StartTime.Value.Minute,
                            overTimeLines[i].StartTime.Value.Second);

                        overTimeLines[i].EndTime = new DateTime(overTimeLines[i].EndDateTime.Value.Year,
                            overTimeLines[i].EndDateTime.Value.Month,
                            overTimeLines[i].EndDateTime.Value.Day,
                            overTimeLines[i].EndTime.Value.Hour,
                            overTimeLines[i].EndTime.Value.Minute,
                            overTimeLines[i].EndTime.Value.Second);

                        overTimeLines[i].TotalDays = 0;

                        if (overTimeLines[i].TotalHours != null)
                            otConsolidated.TotalHours = otConsolidated.TotalHours + overTimeLines[i].TotalHours;


                    }

                    consolidatedLines.Add(otConsolidated);
                }


                return lines;
            }
            catch (Exception exp)
            {
                throw new Exception($"{_OriginalTimesheetRef} Problem mapping Overtime Hours. Rate needs to be other rate type, Rate pay unit should be hourly " + exp.Message, exp);
            }
        }

        private List<IGrouping<string, TimesheetLine>> GetOverTimeDays(out List<TimesheetLine> consolidatedLines)
        {
            try
            {
                consolidatedLines = null;
                if (TimesheetLines == null) return null;

                var lines = TimesheetLines.Where(x => x.Rate.RateType.ToLower() == "other rate" &&
                                                      x.Rate.PayUnit.ToLower() == "daily").GroupBy(x => x.HoursType)
                    .ToList();

                consolidatedLines = new List<TimesheetLine>();
                foreach (var a in lines)
                {
                    var overTimeLines = a.AsEnumerable().ToList();
                    var otConsolidated =
                        JsonConvert.DeserializeObject<TimesheetLine>(
                            JsonConvert.SerializeObject(overTimeLines.FirstOrDefault()));

                    overTimeLines[0].StartTime = new DateTime(overTimeLines[0].StartDateTime.Value.Year,
                        overTimeLines[0].StartDateTime.Value.Month,
                        overTimeLines[0].StartDateTime.Value.Day,
                        overTimeLines[0].StartTime.Value.Hour,
                        overTimeLines[0].StartTime.Value.Minute,
                        overTimeLines[0].StartTime.Value.Second);

                    overTimeLines[0].EndTime = new DateTime(overTimeLines[0].EndDateTime.Value.Year,
                        overTimeLines[0].EndDateTime.Value.Month,
                        overTimeLines[0].EndDateTime.Value.Day,
                        overTimeLines[0].EndTime.Value.Hour,
                        overTimeLines[0].EndTime.Value.Minute,
                        overTimeLines[0].EndTime.Value.Second);

                    if (overTimeLines[0].TotalDays == null)
                    {
                        overTimeLines[0].TotalDays = 0;
                    }

                    if (overTimeLines[0].DaysReported != null)
                    {
                        overTimeLines[0].TotalDays = overTimeLines[0].DaysReported;
                        otConsolidated.TotalDays = overTimeLines[0].DaysReported;

                    }

                    for (var i = 1; i < overTimeLines.Count(); i++)
                    {
                        if (overTimeLines[i].DaysReported != null)
                            otConsolidated.TotalDays = otConsolidated.TotalDays + overTimeLines[i].DaysReported;


                    }

                    consolidatedLines.Add(otConsolidated);
                }


                return lines;
            }
            catch (Exception exp)
            {
                throw new Exception($"{_OriginalTimesheetRef} Problem mapping Overtime daily. Rate needs to be other rate type, Rate pay unit should be daily " + exp.Message, exp);
            }
        }


        public List<RSM.Timesheet> MapTimesheet(ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId, out RSM.ExpenseClaim claim)
        {
            claim = null;

            var timesheetList = new List<RSM.Timesheet>();

            _OriginalTimesheetRef = TimesheetRef;
            TimesheetRef = TimesheetRef;

            var continueMapping = true;

            var timesheet = MapBasicTimesheet();


            //TimesheetLine consolidatedBasicHours = null;
            //var basicHours = GetBasicHours(out consolidatedBasicHours);

            //TimesheetLine consolidatedBasicDays = null;
            //var basicDays = GetBasicDays(out consolidatedBasicDays);

            //List<TimesheetLine> consolidatedOverTimeHours = null;
            //var overTimeHours = GetOverTimeHours(out consolidatedOverTimeHours);

            //List<TimesheetLine> consolidatedOverTimeDays = null;
            //var overTimeDays = GetOverTimeDays(out consolidatedOverTimeDays);

            List<Expense> consolidatedExpenseses = null;
            var expenseList = GetConsolidatedExpenses(out consolidatedExpenseses);




            ////============================== Combine the consolidated lines and map to STI timesheet ============================

            //var combined = new List<TimesheetLine>();

            //if (consolidatedBasicHours != null)
            //{
            //    combined.Add(consolidatedBasicHours);
            //}

            //if (consolidatedBasicDays != null)
            //{
            //    combined.Add(consolidatedBasicDays);
            //}

            //if (consolidatedOverTimeHours != null)
            //{
            //    combined.AddRange(consolidatedOverTimeHours);
            //}

            //if (consolidatedOverTimeDays != null)
            //{
            //    combined.AddRange(consolidatedOverTimeDays);
            //}

            //if (!combined.Any() && !consolidatedExpenseses.Any())
            //{
            //    throw new Exception($"{_OriginalTimesheetRef} - No timesheet lines could be mapped successfully, check rates, hours etc and re-push");
            //}


            if (TimesheetLines.Any())
            {
                timesheet.shifts = new Shift[TimesheetLines.Count];
                var shiftIndex = 0;

                //Map all the time values
                foreach (var line in TimesheetLines)
                {
                    var shift = new RSM.Shift();

                    //get the rate name, default it to Basic Hours first
                    var rateName = "Basic Hours";
                    var rateDescription = "Basic Hours";

                    shift.hoursSpecified = false;
                    shift.daySpecified = false;
                    shift.billInvoiceRequiredSpecified = false;
                    shift.decimalSpecified = false;
                    shift.mealBreakSpecified = false;
                    shift.finishTimeSpecified = false;
                    shift.idSpecified = false;
                    shift.payInvoiceRequiredSpecified = false;
                    shift.salesOnCostValueSpecified = false;
                    shift.rateIdSpecified = false;
                    

                    if (line.Rate.RateType.ToLower() == "basic rate")
                    {
                        if (line.Rate.PayUnit.ToLower() == "daily")
                        {
                            rateName = "Basic Days";
                        }
                    }

                    if (line.Rate.RateType.ToLower() == "other rate")
                    {
                        rateName = line.Rate.FeeName;
                    }

                    shift.rateName = rateName;

                    shift.day = line.StartDateTime.GetDateMilliseconds();
                    shift.daySpecified = true;

                    //Hourly rate
                    if (line.DaysReported == null && line.TotalHours>0)
                    {
                        shift.hours = Convert.ToInt64(line.TotalHours * 60 * 60 * 1000);
                        shift.hoursSpecified = true;

                        shift.startTimeSpecified = true;
                        shift.startTime = line.StartDateTime.GetDateTimeMilliseconds();

                        shift.finishTimeSpecified = true;
                        shift.finishTime = line.EndDateTime.GetDateTimeMilliseconds();

                        shift.mealBreakSpecified = true;

                        if (line.BreakStartTime != null && line.BreakEndTime != null)
                        {
                            var breakStart = line.BreakStartTime.GetDateTimeMilliseconds();
                            var breakEnd = line.BreakEndTime.GetDateTimeMilliseconds();
                            shift.mealBreak = breakEnd - breakStart;
                        }
                        else
                        {
                            shift.mealBreak = 0;
                        }

                    }

                    //Day rate
                    if (line.DaysReported != null && line.DaysReported > 0)
                    {

                        shift.@decimal = (decimal)line.DaysReported;
                        shift.decimalSpecified = true;
                    }

                    timesheet.shifts[shiftIndex] = shift;
                    shiftIndex++;
                }
            }

            
            //map all the expenses
            if (consolidatedExpenseses != null && consolidatedExpenseses.Any())
            {
                claim = new RSM.ExpenseClaim();
                claim.expenseItems = new ExpenseItem[expenseList.Count];
                claim.description = TimesheetRef;
                claim.placementExternalId = AssignmentRef;
                claim.placementIdSpecified = true;

                var index = 0;
                foreach (var expense in consolidatedExpenseses)
                {
                    var expenseItem = new RSM.ExpenseItem();
                    expenseItem.description = expense.Rate.ExpenseType;

                    expenseItem.placementExternalId = AssignmentRef;
                    expenseItem.type = "Expenses";
                    expenseItem.grossValueSpecified = false;
                    expenseItem.exportedSpecified = false;
                    expenseItem.netValueSpecified = false;
                    
                    expenseItem.receiptDateSpecified = true;
                    expenseItem.receiptDate = expense.ExpenseDate;

                    expenseItem.freehandRef = TimesheetRef;
                    expenseItem.payrollRef = TimesheetRef;

                    if (expense.ExpenseType.ToLower() == "mileage")
                    {
                        expenseItem.netValueSpecified = true;
                        expenseItem.netValue = expense.Quantity * expense.Rate.PayRateCurrency;

                        expenseItem.unitNetSpecified = true;
                        expenseItem.unitNet = expense.Rate.PayRateCurrency;

                        expenseItem.unitSpecified = true;
                        expenseItem.unit = expense.Quantity;
                    }
                    else
                    {
                        expenseItem.netValueSpecified = true;
                        expenseItem.netValue = expense.Quantity * expense.Amount;

                        expenseItem.unitNetSpecified = true;
                        expenseItem.unitNet = expense.Amount;
                    }

                    try
                    {
                        expenseItem.payElementCode = rateCodes[expense.ExpenseType];
                    }
                    catch (Exception exp)
                    {
                        logger.Error($"Invalid expense type for timesheet {TimesheetRef} for type {expense.ExpenseType} ", correlationId, this, TimesheetRef, "Timesheet", null, exp, null, null);
                        throw exp;
                    }






                    
                    claim.expenseItems[index] = (expenseItem);
                    index++;
                }
            }

            timesheetList.Add(timesheet);
            
            return timesheetList;
        }

        private RSM.Timesheet MapBasicTimesheet()
        {
            var timesheet = new RSM.Timesheet();

            timesheet.periodEndDateSpecified = false;
            if (PeriodEndDate != null)
            {
                timesheet.periodEndDateSpecified = true;

                var periodEnd = (DateTime) PeriodEndDate;
                timesheet.periodEndDate = periodEnd.Date;
            }

            timesheet.periodStartDateSpecified = false;
            if (PeriodStartDate != null)
            {
                timesheet.periodStartDateSpecified = true;

                var periodStart = (DateTime) PeriodStartDate;
                timesheet.periodStartDate = periodStart.Date;
            }

            timesheet.placementExternalRef = AssignmentRef;

            timesheet.purchaseWrittenOffSpecified = true;
            timesheet.purchaseWrittenOff = false;

            timesheet.salesWrittenOffSpecified = true;
            timesheet.salesWrittenOff = false;

            timesheet.freehandRef = TimesheetRef;
            timesheet.payrollRef = TimesheetRef;

            if (!string.IsNullOrEmpty(ExternalTimesheetId))
            {
                timesheet.payrollRef = ExternalTimesheetId;
            }

            timesheet.purchaseOrderNumOverride = PoNumber;

            DateTime approvedOn = (DateTime) ApprovedDateTime;
            timesheet.comment = "Approved by: " + ApprovedBy + " on " + approvedOn.ToString("dd/MM/yyyy hh:ss");
            
            return timesheet;
        }



    }
}
