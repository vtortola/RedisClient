using SimpleQA.Models;
using SimpleQA.WebApp.Messaging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebSockets;

namespace SimpleQA.WebApp.Controllers
{
    public class PushController : Controller
    {
        [HttpGet]
        public HttpStatusCodeResult Updates(CancellationToken cancel)
        {
            if(System.Web.HttpContext.Current.IsWebSocketRequest)
            {
                System.Web.HttpContext.Current.AcceptWebSocketRequest((context) => ProcessWebSocket(context, cancel));
                return new HttpStatusCodeResult(HttpStatusCode.SwitchingProtocols);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        private async Task ProcessWebSocket(AspNetWebSocketContext context, CancellationToken cancel)
        {
            var websocket = context.WebSocket;

            // I am assuming small messages that will be
            // always smaller than 8K.
            var readBuffer = new Byte[8192];
            var writeBuffer = new Byte[8192];

            using (var messaging = DependencyResolver.Current.GetService<IMessaging>())
            {
                var sending = Task.Run(async () =>
                {
                    while(!websocket.CloseStatus.HasValue && !cancel.IsCancellationRequested)
                    {
                        var message = await messaging.ReceiveMessage(cancel).ConfigureAwait(false);
                        if (message == null)
                            continue;
                        await websocket.SendNotification(writeBuffer, message, cancel).ConfigureAwait(false);
                    }
                }, cancel);

                while (!websocket.CloseStatus.HasValue && !cancel.IsCancellationRequested)
                {
                    var message = await websocket.ReceiveNotification(readBuffer, cancel).ConfigureAwait(false);
                    if (message == null)
                        continue;
                    await messaging.SendMessageAsync(message, cancel).ConfigureAwait(false);
                }
            }
        }
    }
}