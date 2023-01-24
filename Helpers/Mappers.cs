using System;
using System.Collections.Generic;
using System.Text;
using Randstad.OperatingCompanies;

namespace Randstad.UfoRsm.BabelFish.Helpers
{
    public static class Mappers
    {
       

        public static bool MapBool(string value)
        {
            switch (value)
            {
                case "Yes":
                    return true;
                case null:
                case "No":
                    return false;
                default:
                    throw new Exception("Unknown Yes No type");
            }
        }

        public static bool MapCheckin(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            switch (value)
            {
                case "Checked In":
                    return true;
                case "No Show":
                    return false;
                default:
                    throw new Exception("Unknown Yes No type");
            }
        }


        public static OperatingCompany MapOpCoFromName(string value)
        {
            switch (value.ToLower())
            {
                case "executive search":
                case "cpe":
                    return OperatingCompany.CPE;
                case "business solutions":
                    return OperatingCompany.BS;
                case "public services":
                    return OperatingCompany.CARE;
                case "customer success":
                    return OperatingCompany.RIS;
                default: return OperatingCompany.Unknown;
            }
        }


        public static int MapInvoiceDeliveryMethod(string value)
        {
            var defaultValue = 0;

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
                case "self bill":
                    {
                        return 3;
                    }
                default:
                    {
                        return defaultValue;
                    }
            }
        }

        public static string MapGender(string value)
        {
            //TODO: Remove the default once UFO has been updated
            if (string.IsNullOrEmpty(value)) return string.Empty;

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
                        return "M";
                        //throw new Exception("Unknown gender recieved");
                    }
            }
        }



    }
}
