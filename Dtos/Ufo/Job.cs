using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
{
    public class Job : ObjectBase
    {
        public string JobId { get; set; }
        public string MigratedJobId { get; set; }
        public string JobRef { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string JobTitle { get; set; }
        public decimal NumberPlaces { get; set; }
        public string Description { get; set; }
        public double HoursPerWeek { get; set; }

        public ClientContact Contact { get; set; }

        public Owner Owner { get; set; }
        public Address WorkAddress { get; set; }
    }
}