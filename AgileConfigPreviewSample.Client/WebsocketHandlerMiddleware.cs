using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfigPreviewSample.Client
{
    public class SocketPool
    {
        private static List<WebSocket> _clients = new List<WebSocket>();
        public static void Add(WebSocket client)
        {
            ICollection collection = _clients;
            lock (collection.SyncRoot)
            {
                _clients.Add(client);
            }
        }

        public static void Remove(WebSocket client)
        {
            ICollection collection = _clients;
            lock (collection.SyncRoot)
            {
                _clients.Remove(client);
            }
        }

        public static void SendMessage(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);

            ICollection collection = _clients;
            lock (collection.SyncRoot)
            {
                _clients.ForEach(c =>
                {
                    if (c.State == WebSocketState.Open)
                    {
                        c.SendAsync(new ArraySegment<byte>(data, 0, data.Length),
                            WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                });
            }
        }

        public static List<WebSocket> Clients
        {
            get
            {
                return _clients;
            }
        }
    }
    public class WebsocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public WebsocketHandlerMiddleware(
            RequestDelegate next
            )
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    SocketPool.Add(webSocket);

                    try
                    {
                        await Handle(context, webSocket);
                    }
                    catch (Exception ex)
                    {
                        await context.Response.WriteAsync("closed");
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task Handle(HttpContext context, WebSocket client)
        {
            var buffer = new byte[1024 * 2];
            WebSocketReceiveResult result = null;
            do
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = await ConvertWebsocketMessage(result, buffer);
                Console.WriteLine(message);
            }
            while (!result.CloseStatus.HasValue);

            SocketPool.Remove(client);
        }

        private async Task<string> ConvertWebsocketMessage(WebSocketReceiveResult result, ArraySegment<Byte> buffer)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(buffer.Array, buffer.Offset, result.Count);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }

                return "";
            }
        }
    }
}
