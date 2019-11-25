using System;
using System.Collections.Generic;
using ChatCommon.Constants;
using ChatCommon.Messages.Responses;

namespace ChatServer.Extensibility
{
    internal interface IRequestHandlersProvider
    {
        Dictionary<ClientAction, Func<Response>> GetHandlers();
    }
}
