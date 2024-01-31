using System.Threading.Tasks;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Template.Application
{
    internal interface IMessageProcessor
    {
        Task<QueueMessageAction> Process(QueueMessage queueMessage);
    }
}
