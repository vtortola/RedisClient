using Newtonsoft.Json;
using SimpleQA.Messaging;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleQA.WebApp.Messaging
{
    public static class WebSocketNotificationExtensions
    {
        public static Task SendNotification(this WebSocket websocket, Byte[] buffer, PushMessage message, CancellationToken cancel)
        {
            var serializer = new JsonSerializer();
            using (var ms = new MemoryStream(buffer))
            using (var sw = new StreamWriter(ms))
            {
                serializer.Serialize(sw, message);
                sw.Flush();
                return websocket.SendAsync(new ArraySegment<Byte>(buffer, 0, (Int32)ms.Position), WebSocketMessageType.Text, true, cancel);
            }
        }

        public static async Task<PushSubscriptionRequest> ReceiveNotification(this WebSocket websocket, Byte[] buffer, CancellationToken cancel)
        {
            var serializer = new JsonSerializer();
            var asbuffer = new ArraySegment<Byte>(buffer);
            using (var ms = new MemoryStream(buffer))
            {
                WebSocketReceiveResult result = null;
                do
                {
                    result = await websocket.ReceiveAsync(asbuffer, cancel).ConfigureAwait(false);

                    if (result.CloseStatus.HasValue)
                        break;

                    ms.Write(buffer, 0, result.Count);
                }
                while (result != null && !result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                using(var sr = new StreamReader(ms))
                    return serializer.Deserialize<PushSubscriptionRequest>(new JsonTextReader(sr));
            }
        }
    }
}