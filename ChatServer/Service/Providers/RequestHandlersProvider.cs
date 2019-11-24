using System;
using System.Collections.Generic;
using ChatCommon.Actions;
using ChatCommon.Messages.Responses;
using ChatServer.Extensibility;

namespace ChatServer.Service.Providers
{
    class RequestHandlersProvider : IRequestHandlersProvider
    {
        public Dictionary<UserAction, Func<Response>> GetHandlers()
        {
            return new Dictionary<UserAction, Func<Response>>()
            {

            }
        }
        
    }
}
