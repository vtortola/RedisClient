using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace SimpleQA.WebApp.Routing
{
    public class WebSocketRouteConstraint : IRouteConstraint
    {
        readonly Boolean _handleWebSocket;

        public WebSocketRouteConstraint(Boolean handleWebSocket)
        {
            _handleWebSocket = handleWebSocket;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var isWebSocket = HttpContext.Current.IsWebSocketRequest;

            return (_handleWebSocket && isWebSocket) || (!_handleWebSocket && !isWebSocket);
        }
    }
}