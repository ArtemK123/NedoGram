using System;
using System.Collections.Generic;
using ChatCommon.Actions;
using ChatCommon.Messages.Responses;

namespace ChatServer.Extensibility
{
    internal interface IRequestHandlersProvider
    {
        Dictionary<UserAction, Func<Response>> GetHandlers();
    }
}
