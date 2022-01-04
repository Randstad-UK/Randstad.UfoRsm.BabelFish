using System;
using System.Collections.Generic;
using System.Text;
using Randstad.OperatingCompanies;

namespace Randstad.UfoRsm.BabelFish.Helpers
{
    public static class Mappers
    {
        /*
        public static  TrueFalse MapTrueFalse(string value)
        {
            switch (value)
            {
                case "Yes":
                    return TrueFalse.T;
                case "No":
                    return TrueFalse.F;
                default:
                    throw new Exception("Unknown Yes No type");
            }
        }

        public static bool MapBool(string value)
        {
            switch (value)
            {
                case "Yes":
                    return true;
                case "No":
                    return false;
                default:
                    throw new Exception("Unknown Yes No type");
            }
        }

        public static YesNo MapYesNo(string value)
        {

            switch (value)
            {
                case "Yes":
                    return YesNo.Y;
                case "No":
                    return YesNo.N;
                default:
                    throw new Exception("Unknown yes no value");
            }
        }

        public static OperatingCompany MapOpCo(string value)
        {
            switch (value)
            {
                case "CPE":
                    return OperatingCompany.CPE;
                case "BS":
                    return OperatingCompany.BS;
                case "PS":
                    return OperatingCompany.CARE;
                default: return OperatingCompany.Unknown;
            }
        }
        */

        public static OperatingCompany MapOpCoFromName(string value)
        {
            switch (value.ToLower())
            {
                case "cpe":
                    return OperatingCompany.CPE;
                case "business solutions":
                    return OperatingCompany.BS;
                case "public services":
                    return OperatingCompany.CARE;
                default: return OperatingCompany.Unknown;
            }
        }

        public static int MapInvoiceDeliveryMethod(string value)
        {
            var defaultValue = 3;

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            switch (value.ToLower())
            {
                case "electronic":
                {
                    return 1;
                }
                case "paper":
                {
                    return 0;
                }
                default:
                {
                    return defaultValue;
                }
            }
        }

        public static string MapGender(string value)
        {
            switch (value.ToLower())
            {
                case "male":
                {
                    return "M";
                }
                case "female":
                {
                    return "F";
                }
                default:
                {
                    throw new Exception("Unknown gender recieved");
                }
            }
        }
        
    }
}
