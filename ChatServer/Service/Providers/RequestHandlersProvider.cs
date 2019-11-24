using System;
using System.Collections.Generic;
using ChatCommon.Actions;
using ChatCommon.Messages.Responses;
using ChatServer.Extensibility;

namespace ChatServer.Service.Providers
{
    class RequestHandlersProvider : IRequestHandlersProvider
    {
        public Dictionary<ClientAction, Func<Response>> GetHandlers()
        {
            return new Dictionary<ClientAction, Func<Response>>()
            {

            }
        }
        
    }
}
