using NLog;
using SimpleQA.Models;
using SimpleQA.WebApp.Filter;
using SimpleQA.WebApp.Messaging;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebSockets;

namespace SimpleQA.WebApp.Controllers
{
    public class PushController : Controller
    {
        readonly ILogger _logger;

        public PushController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [SimpleQAAuthorize]
        public HttpStatusCodeResult Updates(CancellationToken cancel)
        {
            if(System.Web.HttpContext.Current.IsWebSocketRequest)
            {
                System.Web.HttpContext.Current.AcceptWebSocketRequest((context) => ProcessWebSocket(context, (SimpleQAPrincipal)User, cancel));
                return new HttpStatusCodeResult(HttpStatusCode.SwitchingProtocols);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        private async Task ProcessWebSocket(AspNetWebSocketContext context, SimpleQAPrincipal user, CancellationToken cancel)
        {
            var websocket = context.WebSocket;

            // I am assuming small messages that will be
            // always smaller than 8K.
            var readBuffer = new Byte[8192];
            var writeBuffer = new Byte[8192];

            using (var messaging = DependencyResolver.Current.GetService<IMessaging>())
            {
                messaging.Init(user);

                var sending = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessIncomingMessaging(websocket, messaging, writeBuffer, cancel).ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, "Error processing incoming messaging");
                    }
                }, cancel);

                try
                {
                    await ProcessingOutgoingMessaging(cancel, websocket, readBuffer, messaging).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error processing outgoing messaging");
                }
            }
        }

        static async Task ProcessingOutgoingMessaging(CancellationToken cancel, WebSocket websocket, byte[] readBuffer, IMessaging messaging)
        {
            while (!websocket.CloseStatus.HasValue && !cancel.IsCancellationRequested)
            {
                var message = await websocket.ReceiveNotification(readBuffer, cancel).ConfigureAwait(false);
                if (message == null)
                    continue;
                await messaging.SendMessageAsync(message, cancel).ConfigureAwait(false);
            }
        }

        static async Task ProcessIncomingMessaging(WebSocket websocket, IMessaging messaging, Byte[] writeBuffer, CancellationToken cancel)
        {
            while (!websocket.CloseStatus.HasValue && !cancel.IsCancellationRequested)
            {
                var message = await messaging.ReceiveMessage(cancel).ConfigureAwait(false);
                if (message == null)
                    continue;
                await websocket.SendNotification(writeBuffer, message, cancel).ConfigureAwait(false);
            }
        }
    }
}