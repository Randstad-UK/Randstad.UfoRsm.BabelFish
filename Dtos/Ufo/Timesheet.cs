using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;
using Randstad.OperatingCompanies;
using Randstad.UfoSti.BabelFish.Dtos.Sti;
using Randstad.UfoSti.BabelFish.Helpers;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
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


        //public string ApprovalComment { get; set; }
        //public string ApprovalDescription { get; set; }
        //public DateTime? ApprovedOn { get; set; }
        //public DateTime? SubmittedOn { get; set; }
        //public string AssignmentJobTitle { get; set; }
        //public string ApprovalStatus { get; set; }
        //public string JobCategory { get; set; }
        // public string CandidateRef { get; set; }

        private readonly List<string> timesheetLetter = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
            "V", "W", "X", "Y", "Z"
        };

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

        private void MapInvoiceData(Sti.Timesheet timesheet, List<TimesheetLine> orderedLines)
        {
            timesheet.DailyLines = new List<TimesheetDailyLines>();
            timesheet.GenerateTimesheetImages = true;

            var lineIndex = -1;
            foreach (var line in orderedLines.OrderBy(x => x.StartTime))
            {
                lineIndex++;
                try
                {
                    if (line.Rate == null) continue;

                    //use basic rate name unless OverTimeType populated
                    var rateName = line.Rate.FeeName;
                    if (string.IsNullOrEmpty(rateName))
                        line.Rate.RateType = "Basic Hours";

                    var l = new TimesheetDailyLines();

                    if (line.StartTime != null)
                    {
                        l.StartTime = (DateTime) line.StartTime;
                        l.EndTime = (DateTime) line.EndTime;
                    }

                    if (line.Rate.RateType.ToLower() == "other rate")
                    {
                        l.StartTime = (DateTime) line.StartTime;
                        l.EndTime = (DateTime) line.EndTime;
                    }


                    if (line.BreakTimeMinutes != null)
                        l.LunchTime = (Decimal) line.BreakTimeMinutes;

                    if (line.Rate.PayUnit.ToLower() == "hourly")
                        l.UnitsWorked = (Decimal) line.TotalHours;
                    else if (line.Rate.PayUnit.ToLower() == "daily")
                        l.UnitsWorked = (Decimal) line.DaysReported;

                    timesheet.DailyLines.Add(l);
                }
                catch (Exception exp)
                {
                    throw new Exception($"Problem mapping Invoice Data on TS Line index {lineIndex} "+exp.Message, exp);
                }
            }
        }


        public List<Sti.Timesheet> MapTimesheet(Dictionary<string, string> rateMap, string consultantPrefixCode, Dictionary<string, string> tomCodes, Dictionary<string, string> employerRefs)
        {
            var timesheetList = new List<Sti.Timesheet>();

            _OriginalTimesheetRef = TimesheetRef;
            TimesheetRef = TimesheetRef.Replace("TSM-", "UT");

            var continueMapping = true;

            var timesheet = MapBasicTimesheet(consultantPrefixCode, tomCodes, employerRefs);

            MapConsultantSplit(timesheet, consultantPrefixCode);

            TimesheetLine consolidatedBasicHours = null;
            var basicHours = GetBasicHours(out consolidatedBasicHours);

            TimesheetLine consolidatedBasicDays = null;
            var basicDays = GetBasicDays(out consolidatedBasicDays);

            List<TimesheetLine> consolidatedOverTimeHours = null;
            var overTimeHours = GetOverTimeHours(out consolidatedOverTimeHours);

            List<TimesheetLine> consolidatedOverTimeDays = null;
            var overTimeDays = GetOverTimeDays(out consolidatedOverTimeDays);

            List<Expense> consolidatedExpenseses = null;
            var expenseList = GetConsolidatedExpenses(out consolidatedExpenseses);


            //============================== Combine the seperate lines order them and generate invoice data ============================
            var orderedLines = new List<TimesheetLine>();
            
            if(basicHours!=null)
                orderedLines.AddRange(basicHours);


            if (basicDays != null)
            {
                orderedLines.AddRange(basicDays);
            }

            if (overTimeHours != null)
            {
                foreach (var a in overTimeHours)
                {
                    orderedLines.AddRange(a.AsEnumerable().ToList());
                }
            }


            if (overTimeDays != null)
            {
                foreach (var a in overTimeDays)
                {
                    orderedLines.AddRange(a.AsEnumerable().ToList());
                }
            }

            MapInvoiceData(timesheet, orderedLines);

            //============================== End invoice data generate ==================================================================


            //============================== Combine the consolidated lines and map to STI timesheet ============================

            var combined = new List<TimesheetLine>();

            if (consolidatedBasicHours != null)
            {
                combined.Add(consolidatedBasicHours);
            }

            if (consolidatedBasicDays != null)
            {
                combined.Add(consolidatedBasicDays);
            }

            if (consolidatedOverTimeHours != null)
            {
                combined.AddRange(consolidatedOverTimeHours);
            }

            if (consolidatedOverTimeDays != null)
            {
                combined.AddRange(consolidatedOverTimeDays);
            }

            if (!combined.Any() && !consolidatedExpenseses.Any())
            {
                throw new Exception($"{_OriginalTimesheetRef} - No timesheet lines could be mapped successfully, check rates, hours etc and re-push");
            }

            var timesheetCount = -1;
            while (continueMapping)
            {
                var tsl = combined.Where(x => x.IsMapped == false).Take(5);

                List<Expense> expenses = null;
                if (Expenses != null)
                {
                    expenses = consolidatedExpenseses.Where(x => x.IsMapped == false).Take(3).ToList();
                }

                //no more timesheet lines or expenses
                if (!tsl.Any() && (expenses ==null || !expenses.Any()))
                {
                    continueMapping = false;
                    continue;
                }

                //generate a new timesheet instance and increment the timesheet ref
                if (timesheetCount > -1)
                {
                    TimesheetRef = TimesheetRef + timesheetLetter[timesheetCount];
                    timesheet = MapBasicTimesheet(consultantPrefixCode, tomCodes, employerRefs);
                }

                //Map the timesheet lines to timesheet non adhoc
                var lineCount = 0;
                foreach (var t in tsl)
                {
                    if (!string.IsNullOrEmpty(t.PoNumber))
                    {
                        timesheet.PurchaseOrderNumber = t.PoNumber;
                    }

                    if(TotalDays>0)
                        MapLine(timesheet, t, lineCount, rateMap, false);
                    else
                        MapLine(timesheet, t, lineCount, rateMap, true);

                    t.IsMapped = true;
                    lineCount++;

                }

                //Map the expenses to timesheet adhoc sections
                var adhocCount = 0;
                if (expenses != null)
                {
                    foreach (var e in expenses)
                    {
                        MapExpenseLine(timesheet, e, adhocCount, rateMap);
                        e.IsMapped = true;
                        adhocCount++;
                    }
                }

                //set the location
                if (WorkAddress != null)
                {
                    timesheet.WorkLocation = WorkAddress.City;
                    if (!string.IsNullOrEmpty(WorkAddress.County))
                    {
                        timesheet.WorkLocation += "," + WorkAddress.County;
                    }
                }

                timesheet.JobLocation = timesheet.WorkLocation;
                timesheet.JobDescription = JobTitle;
                timesheet.SignedOff = YesNo.Y;
                timesheet.EntityReference = TimesheetRef;
                timesheet.TimesheetNumber = TimesheetRef;
                timesheetList.Add(timesheet);
                timesheetCount++;
            }

            

            return timesheetList;
        }

        private Sti.Timesheet MapBasicTimesheet(string consultantPrefixCode, Dictionary<string, string> tomCodes, Dictionary<string, string> employerRefs)
        {
            var timesheet = new Sti.Timesheet();
            timesheet.ClientRef = ClientRef;
            timesheet.PersonnelRef = PayrollRef;
            timesheet.ClientName = ClientName;
            timesheet.WorkerName = CandidateName;

            //Map all the finance codes-------------------------------------------------
            //Map OpCo
            try
            {
                if (!string.IsNullOrEmpty(OpCo.FinanceCode))
                    timesheet.OpCo = Mappers.MapOpCo(OpCo.FinanceCode);
                else
                    timesheet.OpCo = Mappers.MapOpCoFromName(OpCo.Name);
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping OpCo for timesheet", exp);
            }

            //Map Division
            try
            {
                timesheet.Division = tomCodes[Unit.FinanceCode];
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping Division for timesheet");
            }

            //Map EmployerRef
            try
            {
                timesheet.EmployerRef = employerRefs[OpCo.FinanceCode];
            }
            catch (Exception exp)
            {
                throw new Exception("Problem mapping employer ref for timesheet", exp);
            }

            //Map department
            timesheet.Department = Unit.FinanceCode;

            //Finished mapping finance codes--------------------------------------------

            timesheet.InvoiceToClient = ClientRef;

            if (timesheet.OpCo == OperatingCompany.CPE && HleRef != null)
            {
                timesheet.ReportToClient = HleRef;
                timesheet.InvoiceToClient = ClientRef;
            }

            if (PeriodEndDate != null)
            {
                timesheet.TimesheetDate = (DateTime)PeriodStartDate;

                if (timesheet.OpCo == OperatingCompany.CPE)
                {
                    var num_days = DayOfWeek.Friday - timesheet.TimesheetDate.DayOfWeek;

                    if (num_days < 0)
                        num_days += 7;

                    timesheet.TimesheetDate = timesheet.TimesheetDate.AddDays(num_days);
                }
                else
                {
                    var num_days = DayOfWeek.Sunday - timesheet.TimesheetDate.DayOfWeek;

                    if (num_days < 0)
                        num_days += 7;

                    timesheet.TimesheetDate = timesheet.TimesheetDate.AddDays(num_days);
                }
            }

            timesheet.EntityReference = TimesheetRef;
            timesheet.TimesheetNumber = TimesheetRef;

            timesheet.PurchaseOrderNumber = PoNumber;

            //Consultant split[0] is always the owning consultant, user their employer ref but needs to be prefixed with Code from config
            timesheet.ConsultantCode = consultantPrefixCode + ConsultantSplits[0].Consultant.EmployeeRef;



            timesheet.AssignmentRef = AssignmentRef;
            timesheet.BookedBy = Contact;
            timesheet.Adjustment = YesNo.N;

            timesheet.Costcentre = CostCentre;
            timesheet.ExternalTSId = TimesheetId;

            if (!string.IsNullOrEmpty(ApprovedBy) && ApprovedDateTime != null)
            {
                timesheet.AuthorisedBy = ApprovedBy;
                timesheet.AuthorisedDate = (DateTime)ApprovedDateTime;
            }

            if (string.IsNullOrEmpty(PaymentType))
            {
                throw new Exception("Payment type is not set on the Employee File");
            }

            switch (PaymentType.ToLower())
            {
                case "ltd":
                    {
                        timesheet.SupplierName = LtdCompany;
                        break;
                    }
                case "umbrella":
                    {
                        timesheet.SupplierName = Umbrella;
                        break;
                    }
                case "outsourced":
                    {
                        timesheet.SupplierName = Outsourced;
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(InvoiceAddressId))
            {
                timesheet.InvoiceAddressNumber = int.Parse(InvoiceAddressId);
            }

            return timesheet;
        }

        private void MapLine(Sti.Timesheet timesheet, TimesheetLine line, int timesheetLineIndex, Dictionary<string, string> rateMap, bool isHourly)
        {
            if (line.Rate == null) return;


            //get the rate name, default it to Basic Hours first
            var rateName = "Basic Hours";
            var rateDescription = "Basic Hours";
            var frequency = RateFrequency.H;

            if (line.Rate.RateType.ToLower() == "basic rate")
            {
                if (line.Rate.PayUnit.ToLower() == "daily")
                {
                    rateDescription = "Basic Days";
                    frequency = RateFrequency.D;
                }
            }

            /****************** HACK TO GET PAYROLL THROUGH  ********************************************
            if (!isHourly)
            {
                rateName = "Basic Days";
            }
            else
            {
                rateName = "Basic Hours";
            }
            ****************** END HACK TO GET PAYROLL THROUGH  *********************************************/

            if (line.Rate.RateType.ToLower() == "other rate")
            {
                rateName = line.Rate.OvertimeType;
                rateDescription = line.Rate.FeeName;
            }

            switch (timesheetLineIndex)
            {

                case 0:
                {
                    timesheet.RateCode1 = rateMap[rateName];
                    timesheet.Description1 = rateDescription;
                    timesheet.Line1Frequency = frequency;
                    //Hourly rate
                    if (line.TotalDays == null || line.TotalDays <= 0)
                    {
                        timesheet.Hours1 = line.TotalHours;
                    }
                    //Day rate
                    else
                    {
                        timesheet.Hours1 = (decimal) line.TotalDays;
                    }

                    timesheet.PayRate1 = line.Rate.PayRateCurrency;
                    timesheet.BillRate1 = line.Rate.ChargeRateCurrency;
       
                    if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
                    {
                        timesheet.Basic1AccrueWTD = YesNo.N;
                    }
     
                    break;
                }
                case 1:
                {
                    timesheet.RateCode2 = rateMap[rateName];
                    timesheet.Description2 = rateDescription;
                    timesheet.Line2Frequency = frequency;
                    //Hourly rate
                    if (line.TotalDays == null || line.TotalDays <= 0)
                    {
                        timesheet.Hours2 = line.TotalHours;
                    }
                        //Day rate
                        else
                    {
                        timesheet.Hours2 = (decimal)line.TotalDays;
                    }

                    timesheet.PayRate2 = line.Rate.PayRateCurrency;
                    timesheet.BillRate2 = line.Rate.ChargeRateCurrency;

                    if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
                    {
                        timesheet.Basic2AccrueWTD = YesNo.N;
                    }

                    break;
                }
                case 2:
                {
                    timesheet.RateCode3 = rateMap[rateName];
                    timesheet.Description3 = rateDescription;
                    timesheet.Line3Frequency = frequency;
                    //Hourly rate
                    if (line.TotalDays == null || line.TotalDays <= 0)
                    {
                        timesheet.Hours3 = line.TotalHours;
                    }
                    //Day rate
                    else
                    {
                        timesheet.Hours3 = (decimal)line.TotalDays;
                    }

                    timesheet.PayRate3 = line.Rate.PayRateCurrency;
                    timesheet.BillRate3 = line.Rate.ChargeRateCurrency;

                    if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
                    {
                        timesheet.Basic3AccrueWTD = YesNo.N;
                    }

                    break;
                }
                case 3:
                {
                    timesheet.RateCode4 = rateMap[rateName];
                    timesheet.Description4 = rateDescription;
                    timesheet.Line4Frequency = frequency;
                    //Hourly rate
                    if (line.TotalDays == null || line.TotalDays <= 0)
                    {
                        timesheet.Hours4 = line.TotalHours;
                    }
                    //Day rate
                    else
                    {
                        timesheet.Hours4 = (decimal)line.TotalDays;
                    }

                    timesheet.PayRate4 = line.Rate.PayRateCurrency;
                    timesheet.BillRate4 = line.Rate.ChargeRateCurrency;

                    if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
                    {
                        timesheet.Basic4AccrueWTD = YesNo.N;
                    }

                    break;
                }
                case 4:
                {
                    timesheet.RateCode5 = rateMap[rateName];
                    timesheet.Description5 = rateDescription;
                    timesheet.Line5Frequency = frequency;
                    //Hourly rate
                    if (line.TotalDays == null || line.TotalDays <= 0)
                    {
                        timesheet.Hours5 = line.TotalHours;
                    }
                    //Day rate
                    else
                    {
                        timesheet.Hours5 = (decimal)line.TotalDays;
                    }

                    timesheet.PayRate5 = line.Rate.PayRateCurrency;
                    timesheet.BillRate5 = line.Rate.ChargeRateCurrency;

                    if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
                    {
                        timesheet.Basic5AccrueWTD = YesNo.N;
                    }

                    break;
                }
                default:
                    throw new Exception("Too many timesheet lines");

            }
        }

        private void MapExpenseLine(Sti.Timesheet timesheet, Expense expense, int adhocIndex, Dictionary<string, string> rateMap)
        {
            switch(adhocIndex)
            {
                case 0:
                {
                    timesheet.AdhocCode1 = rateMap[expense.Rate.ExpenseType];
                    timesheet.Adhoc1Description = expense.Rate.ExpenseType;

                    if (expense.Rate.PayRateCurrency != null)
                        timesheet.Adhoc1PayRate = (decimal) expense.Rate.PayRateCurrency;

                    if (expense.Rate.ChargeRateCurrency != null)
                        timesheet.Adhoc1BillRate = (decimal) expense.Rate.ChargeRateCurrency;

                    if (expense.Quantity != null)
                        timesheet.Adhoc1Hours = (decimal) expense.Quantity;

                    if (expense.Amount != null)
                    {
                        timesheet.Adhoc1PayRate = (decimal) expense.Amount;
                        timesheet.Adhoc1BillRate = timesheet.Adhoc1PayRate;
                    }

                    break;
                }
                case 1:
                {
                    timesheet.AdhocCode2 = rateMap[expense.Rate.ExpenseType];
                    timesheet.Adhoc2Description = expense.Rate.ExpenseType;

                    if (expense.Rate.PayRateCurrency != null)
                        timesheet.Adhoc2PayRate = (decimal) expense.Rate.PayRateCurrency;

                    if (expense.Rate.ChargeRateCurrency != null)
                        timesheet.Adhoc2BillRate = (decimal) expense.Rate.ChargeRateCurrency;

                    if (expense.Quantity != null)
                        timesheet.Adhoc2Hours = (decimal) expense.Quantity;

                    if (expense.Amount != null)
                    {
                        timesheet.Adhoc2PayRate = (decimal) expense.Amount;
                        timesheet.Adhoc2BillRate = timesheet.Adhoc2PayRate;
                    }

                    break;
                }
                case 2:
                {
                    timesheet.AdhocCode3 = rateMap[expense.Rate.ExpenseType];
                    timesheet.Adhoc3Description = expense.Rate.ExpenseType;

                    if (expense.Rate.PayRateCurrency != null)
                        timesheet.Adhoc3PayRate = (decimal) expense.Rate.PayRateCurrency;

                    if (expense.Rate.ChargeRateCurrency != null)
                        timesheet.Adhoc3BillRate = (decimal) expense.Rate.ChargeRateCurrency;

                    if (expense.Quantity != null)
                        timesheet.Adhoc3Hours = (decimal) expense.Quantity;

                    if (expense.Amount != null)
                    {
                        timesheet.Adhoc3PayRate = (decimal) expense.Amount;
                        timesheet.Adhoc3BillRate = timesheet.Adhoc3PayRate;
                    }

                    break;
                }
                default:
                    throw new Exception("Too many expenses added to Timesheet");
            }

        }

        private void MapConsultantSplit(Sti.Timesheet timesheet, string consultantPrefixCode)
        {
            if (ConsultantSplits== null || ConsultantSplits.Count <= 1) return;

            timesheet.RZConsultant1Split = ConsultantSplits[0].Split;
            timesheet.RZConsultantCode1 = consultantPrefixCode+ConsultantSplits[0].Consultant.EmployeeRef;
            
            timesheet.RZConsultant2Split = ConsultantSplits[1].Split;
            timesheet.RZConsultantCode2 = consultantPrefixCode+ConsultantSplits[1].Consultant.EmployeeRef;
        }
    }
}
