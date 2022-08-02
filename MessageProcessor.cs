using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.UfoRsm.BabelFish.Template.Application;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Settings;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish
{
    internal class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger _logger;
        private readonly List<ITranslator> _translators;

        public MessageProcessor(ILogger logger, List<ITranslator> translators, ApplicationSettings appSettings)
        {
            _logger = logger;
            _translators = translators;
        }

        /// <summary>
        /// Process the next message and return the action to be taken.
        /// </summary>
        /// <param name="queueMessage">The message consumed from the RabbitMQ queue. It will never be null.</param>
        /// <returns>A <see cref="QueueMessageAction"/> which is the action the <see cref="MessageConsumer"/> should perform on the message.</returns>
        public async Task<QueueMessageAction> Process(QueueMessage queueMessage)
        {
            // This is here to prevent a compiler warning. To make async calls in
            // this method then remove this as you'll be awaiting something else.
            await Task.CompletedTask;

            var entity = JsonConvert.DeserializeObject<ExportedEntity>(queueMessage.Body);

            foreach (var t in _translators)
            {
                await t.Translate(entity);
            }

            //failed to export must log warning and acknowledge message
            if (!entity.ExportSuccess)
            {
                return QueueMessageAction.Acknowledge;
            }

            return QueueMessageAction.Acknowledge;
        }
    }
}
