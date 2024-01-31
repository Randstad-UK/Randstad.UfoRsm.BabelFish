using System.Collections.Generic;

namespace Randstad.UfoRsm.BabelFish.Template.Settings
{
    internal class ConsumerServiceSettings
    {
        public string QueueName { get; set; }
        public List<string> Bindings { get; set; }
    }
}
