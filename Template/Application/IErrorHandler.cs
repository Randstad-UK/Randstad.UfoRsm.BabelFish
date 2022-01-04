using System;

namespace Randstad.UfoRsm.BabelFish.Template.Application
{
    internal interface IErrorHandler
    {
        void ResetKnownErrorsCount();
        bool Handle(Exception ex, Guid correlationId);
    }
}