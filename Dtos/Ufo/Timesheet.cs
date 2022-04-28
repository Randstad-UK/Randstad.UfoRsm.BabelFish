using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Transactions;
using Microsoft.Extensions.Configuration;
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


        public string PoNumber { get; set; }

        public string TimesheetRef { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public string ExternalTimesheetId { get; set; }

        public Team OpCo { get; set; }

    
        public List<TimesheetLine> TimesheetLines { get; set; }
        public List<Expense> Expenses { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public bool ProcessAdjustments { get; set; }


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


        public List<RSM.Timesheet> MapTimesheet(ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId, out RSM.ExpenseClaim claim)
        {
            claim = null;

            var timesheetList = new List<RSM.Timesheet>();

            TimesheetRef = TimesheetRef;

            var timesheet = MapBasicTimesheet();
            
            List<Expense> consolidatedExpenseses = null;
            var expenseList = GetConsolidatedExpenses(out consolidatedExpenseses);

            if (TimesheetLines!=null && TimesheetLines.Any())
            {
                //timesheet.shifts = new Shift[TimesheetLines.Count];
                timesheet.shifts = new Shift[0];
                var shiftIndex = 0;
                var shiftList = new List<Shift>();
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
                    

                    if (line.Rate!=null && line.Rate.RateType.ToLower() == "basic rate")
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


                    Rate pRate = null;
                    if (line.Rate != null)
                    {
                        if (line.Rate.Assignment == null)
                        {
                            line.Rate.Assignment = new Assignment();
                            line.Rate.Assignment.AssignmentRef = AssignmentRef;
                        }
                        shift.rate = line.Rate.MapRate(rateCodes, out pRate);
                    }


                    var date = (DateTime) line.StartDateTime.ConvertToBST();
                    shift.day = date.Date.GetDateTimeMilliseconds();
                    shift.daySpecified = true;
                    shift.rateName = rateName;

                    //Hourly rate
                    if (line.DaysReported == null && line.TotalHours>0)
                    {
                        shift.hours = Convert.ToInt64(line.TotalHours * 60 * 60 * 1000);
                        shift.hoursSpecified = true;

                        shift.startTimeSpecified = true;
                        var startDate = line.StartDateTime.ConvertToBST();
                        logger.Debug("Local Start Date/Time: "+ startDate, correlationId, this, TimesheetRef, null, null, null, null);
                        shift.startTime = startDate.GetDateTimeMilliseconds();

                        shift.finishTimeSpecified = true;

                        var endDate = line.EndDateTime.ConvertToBST();
                        logger.Debug("Local End Date/Time: " + endDate, correlationId, this, TimesheetRef, null, null, null, null);
                        shift.finishTime = endDate.GetDateTimeMilliseconds();

                        shift.mealBreakSpecified = true;

                        if (line.BreakStartTime != null && line.BreakEndTime != null)
                        {
                            var breakStartBST = line.BreakStartTime.ConvertToBST();
                            logger.Debug("Local Start Break Date/Time: " + breakStartBST, correlationId, this, TimesheetRef, null, null, null, null);
                            var breakStart = breakStartBST.GetDateTimeMilliseconds();

                            var breakEndBST = line.BreakEndTime.ConvertToBST();
                            logger.Debug("Local End Break Date/Time: " + breakEndBST, correlationId, this, TimesheetRef, null, null, null, null);
                            var breakEnd = breakEndBST.GetDateTimeMilliseconds();
                            shift.mealBreak = breakEnd - breakStart;
                        }
                        else
                        {
                            shift.mealBreak = 0;
                        }

                        shiftList.Add(shift);
                        shiftIndex++;
                    }

                    //Day rate
                    if (line.DaysReported != null && line.DaysReported > 0)
                    {

                        shift.@decimal = (decimal)line.DaysReported;
                        shift.decimalSpecified = true;
                        shiftList.Add(shift);
                        shiftIndex++;
                    }
                }

                if (shiftList.Any())
                {
                    timesheet.shifts = shiftList.ToArray();
                }
                timesheetList.Add(timesheet);
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
                    expenseItem.receiptDate = PeriodEndDate.ConvertToBST().Date;

                    expenseItem.freehandRef = TimesheetRef;
                    expenseItem.payrollRef = TimesheetRef;

                    if (expense.ExpenseType.ToLower() == "mileage")
                    {
                        expenseItem.type = "Mileage";
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

                        expenseItem.unitSpecified = true;
                        expenseItem.unit = expense.Quantity;
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

            
            
            return timesheetList;
        }

        private RSM.Timesheet MapBasicTimesheet()
        {
            var timesheet = new RSM.Timesheet();

            timesheet.periodEndDateSpecified = false;
            if (PeriodEndDate != null)
            {
                timesheet.periodEndDateSpecified = true;

                timesheet.periodEndDate = PeriodEndDate.ConvertToBST().Date;
            }

            timesheet.periodStartDateSpecified = false;
            if (PeriodStartDate != null)
            {
                timesheet.periodStartDateSpecified = true;
                timesheet.periodStartDate = PeriodStartDate.ConvertToBST().Date;
            }

            timesheet.placementExternalRef = AssignmentRef;

            timesheet.purchaseWrittenOffSpecified = true;
            timesheet.purchaseWrittenOff = false;

            timesheet.salesWrittenOffSpecified = true;
            timesheet.salesWrittenOff = false;

            timesheet.freehandRef = TimesheetRef;
            timesheet.payrollRef = TimesheetRef;
            timesheet.externalTimesheetId = ExternalTimesheetId;

            timesheet.purchaseOrderNumOverride = PoNumber;

            DateTime approvedOn = (DateTime) ApprovedDateTime.ConvertToBST();
            timesheet.comment = "Approved by: " + ApprovedBy + " on " + approvedOn.ToString("dd/MM/yyyy hh:ss");
            
            return timesheet;
        }



    }
}
