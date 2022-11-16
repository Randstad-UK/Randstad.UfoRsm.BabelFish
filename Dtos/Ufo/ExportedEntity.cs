using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class ExportedEntity
    {
        public string Id { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime ExportDate { get; set; }
        public string EventType { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string Payload { get; set; }
        public bool IsCheckedIn { get; set; }
        public List<string> ValidationErrors { get; set; }
        public bool ExportSuccess { get; set; }
        public string ReceivedOnRoutingKey { get; set; }
        public string[] ReceivedOnRoutingKeyNodes { get; set; }
    }
}
