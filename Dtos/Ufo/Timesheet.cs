using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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
        public DateTime AssignmentStart { get; set; }
        public bool Cancelled { get; set; }
        public string PoNumber { get; set; }
        public string TimesheetRef { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ExternalTimesheetId { get; set; }

        public Team OpCo { get; set; }
        public Team Division { get; set; }


        public List<TimesheetLine> TimesheetLines { get; set; }
        public List<Expense> Expenses { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedDateTime { get; set; }
        public bool? ProcessAdjustments { get; set; }

        //Student Support
        public string StudentFirstName { get; set; }
        public string StudentSurname { get; set; }


        private void AddExpenseTypeLines(List<RSM.Shift> shifts)
        {

            if (Expenses == null) return;


            foreach (var exp in Expenses)
            {
                if (exp.ExpenseType == "Bonus" || exp.ExpenseType == "Back Pay - Non WTR" || exp.ExpenseType == "Back Pay - WTR")
                {
                    var shift = new RSM.Shift();

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

                    shift.rateName = exp.ExpenseType;
                    shift.daySpecified = true;
                    shift.day = EndDate.GetDateTimeMilliseconds();
                    shift.rateName = exp.Rate.FeeName;
                    shift.@decimal = (decimal)exp.Quantity;
                    shift.decimalSpecified = true;
                    shifts.Add(shift);
                }
            }

        }


        public List<RSM.Timesheet> MapTimesheet(ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId, out RSM.ExpenseClaim claim)
        {
            claim = null;

            var timesheetList = new List<RSM.Timesheet>();

            TimesheetRef = TimesheetRef;

            var timesheet = MapBasicTimesheet();


            var shiftList = new List<Shift>();

            //add the expense type lines
            AddExpenseTypeLines(shiftList);
            if (TimesheetLines != null && TimesheetLines.Any())
            {
                timesheet.shifts = new Shift[0];
                var shiftIndex = 0;


                //removed as no longer needed
                /*
                if ((Division.Name == "Tuition Services" || Division.Name == "Student Support") && Unit.Name=="NTP Tuition Pillar")
                {
                    TimesheetLines.Add(GetDeduction(30, "DFE Subsidy - 70%"));
                }*/

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

                    if (line.Rate != null && line.Rate.RateType.ToLower() == "basic rate")
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

                    var day = DateTime.Parse(line.StartDateTime);
                    var dayDate = day.Date;
                    shift.day = dayDate.ToString().GetDateTimeMilliseconds();

                    shift.daySpecified = true;
                    shift.rateName = rateName;

                    if (Division.Name == "Tuition Services" || Division.Name == "Student Support")
                    {
                        shift.comment = StudentFirstName + " " + StudentSurname;
                    }


                    //Hourly rate
                    if (line.DaysReported == null && (line.TotalHours > 0 || line.TotalHours < 0))
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
            }

            if (TimesheetRef.StartsWith("NT") || shiftList.Any())
            {
                timesheet.shifts = shiftList.ToArray();
                timesheetList.Add(timesheet);
            }

            //map all the expenses
            if (Expenses != null && Expenses.Any())
            {
                claim = new RSM.ExpenseClaim();

                claim.description = TimesheetRef;
                claim.placementExternalId = AssignmentRef;
                claim.placementIdSpecified = true;

                List<ExpenseItem> items = new List<ExpenseItem>();

                foreach (var expense in Expenses)
                {
                    if (expense.ExpenseType == "Bonus" || expense.ExpenseType == "Back Pay - Non WTR" || expense.ExpenseType == "Back Pay - WTR") continue;

                    var expenseItem = new RSM.ExpenseItem();
                    expenseItem.description = expense.Rate.ExpenseType;

                    expenseItem.placementExternalId = AssignmentRef;
                    expenseItem.type = expense.Rate.ExpenseType;
                    expenseItem.grossValueSpecified = false;
                    expenseItem.exportedSpecified = false;
                    expenseItem.netValueSpecified = false;

                    expenseItem.receiptDateSpecified = true;

                    if (EndDate != null)
                    {
                        expenseItem.receiptDate = DateTime.Parse(EndDate).Date;
                    }
                    else
                    {
                        expenseItem.receiptDate = DateTime.Parse(ApprovedDateTime).Date;
                    }

                    if (Division.Name == "Tuition Services" || Division.Name == "Student Support")
                    {
                        expenseItem.freehandRef = StudentFirstName + " " + StudentSurname;
                    }

                    if (TimesheetRef.StartsWith("NT"))
                    {
                        expenseItem.freehandRef = ExternalTimesheetId;
                    }


                    expenseItem.payrollRef = TimesheetRef;

                    if (expense.ExpenseType.ToLower() == "mileage")
                    {
                        expenseItem.type = expense.ExpenseType;
                        expenseItem.netValueSpecified = true;
                        expenseItem.netValue = expense.Quantity * expense.Rate.PayRateCurrency;

                        expenseItem.grossValueSpecified = true;
                        expenseItem.grossValue = expenseItem.netValue;

                        expenseItem.unitNetSpecified = true;
                        expenseItem.unitNet = expense.Rate.PayRateCurrency;

                        expenseItem.unitSpecified = true;
                        expenseItem.unit = expense.Quantity;
                    }
                    else
                    {
                        expenseItem.netValueSpecified = true;
                        expenseItem.netValue = expense.Quantity * expense.Amount;

                        expenseItem.grossValueSpecified = true;
                        expenseItem.grossValue = expenseItem.netValue;

                        expenseItem.unitNetSpecified = true;
                        expenseItem.unitNet = expense.Amount;

                        expenseItem.unitSpecified = true;
                        expenseItem.unit = expense.Quantity;
                    }

                    try
                    {
                        expenseItem.payElementCode = rateCodes[expense.ExpenseType];

                        if (expense.Rate.NonChargeableToClient)
                        {
                            expenseItem.payElementCode = expenseItem.payElementCode + "NC";
                            expenseItem.type = expense.Rate.ExpenseType +" NC";
                            expenseItem.description = expenseItem.type;
                        }
                    }
                    catch (Exception exp)
                    {
                        logger.Error($"Invalid expense type for timesheet {TimesheetRef} for type {expense.ExpenseType} ", correlationId, this, TimesheetRef, "Timesheet", null, exp, null, null);
                        throw exp;
                    }


                    items.Add(expenseItem);
                }

                if (items.Any())
                {
                    claim.expenseItems = items.ToArray();
                }
            }



            return timesheetList;
        }

        private RSM.Timesheet MapBasicTimesheet()
        {
            var timesheet = new RSM.Timesheet();

            timesheet.periodEndDateSpecified = false;
            if (EndDate != null)
            {
                timesheet.periodEndDateSpecified = true;

                DateTime periodEnd = DateTime.Parse(EndDate);
                timesheet.periodEndDate = periodEnd.Date;
            }

            timesheet.periodStartDateSpecified = false;
            if (StartDate != null)
            {
                timesheet.periodStartDateSpecified = true;

                DateTime periodStart = DateTime.Parse(StartDate);
                timesheet.periodStartDate = periodStart.Date;
            }

            timesheet.placementExternalRef = AssignmentRef;

            timesheet.purchaseWrittenOffSpecified = true;
            timesheet.purchaseWrittenOff = false;

            timesheet.salesWrittenOffSpecified = true;
            timesheet.salesWrittenOff = false;

            if (Division.Name == "Tuition Services" || Division.Name == "Student Support")
            {
                timesheet.freehandRef = StudentFirstName + " " + StudentSurname;
            }

            if (TimesheetRef.StartsWith("NT"))
            {
                timesheet.freehandRef = ExternalTimesheetId;
            }

            timesheet.payrollRef = TimesheetRef;
            timesheet.externalTimesheetId = ExternalTimesheetId;

            if (!string.IsNullOrEmpty(PoNumber))
            {
                timesheet.purchaseOrderNumOverride = PoNumber.Trim();
            }

            DateTime approvedOn = DateTime.Parse(ApprovedDateTime);
            timesheet.comment = "Approved by: " + ApprovedBy + " on " + approvedOn.ToString("dd/MM/yyyy hh:ss");


            return timesheet;
        }

        private TimesheetLine GetDeduction(decimal percentageDeduction, string feeName)
        {
            var basic = TimesheetLines.Where(x => x.Rate.RateType.ToLower() == "basic rate").ToList();

            if (basic == null) return null;

            var deduction = new TimesheetLine();
            deduction.StartDateTime = basic[0].StartDateTime;
            deduction.EndDateTime = basic[0].EndDateTime;
            deduction.StartDateTime = basic[0].StartDateTime;
            deduction.EndDateTime = basic[0].EndDateTime;
            deduction.HoursType = basic[0].HoursType;
            deduction.PoNumber = basic[0].PoNumber;
            deduction.TotalHours = 0;
            foreach (var line in basic)
            {
                deduction.TotalHours = deduction.TotalHours + line.TotalHours;

            }

            deduction.TotalHours = deduction.TotalHours * -1;

            deduction.Rate = new AssignmentRate();
            deduction.Rate.FeeName = feeName; //"DFE Subsidy -70%";
            deduction.Rate.OvertimeType = feeName;
            deduction.Rate.PayRateCurrency = 0;
            deduction.Rate.PayUnit = basic[0].Rate.PayUnit;
            deduction.Rate.StartDate = AssignmentStart.ToString();

            var charge = (decimal)basic[0].Rate.ChargeRateCurrency;
            var percentage = percentageDeduction / 100;
            charge = charge - (charge * percentage);
            deduction.Rate.ChargeRateCurrency = Math.Round(charge, 2, MidpointRounding.AwayFromZero);
            deduction.Rate.RateType = "Other Rate";
            return deduction;

        }



    }
}
