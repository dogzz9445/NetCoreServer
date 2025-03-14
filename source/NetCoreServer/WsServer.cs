﻿using System.Net;
using System.Text;

namespace NetCoreServer
{
    /// <summary>
    /// WebSocket server
    /// </summary>
    /// <remarks> WebSocket server is used to communicate with clients using WebSocket protocol. Thread-safe.</remarks>
    public class WsServer : HttpServer, IWebSocket
    {
        internal readonly WebSocket WebSocket;

        /// <summary>
        /// Initialize WebSocket server with a given IP address and port number
        /// </summary>
        /// <param name="address">IP address</param>
        /// <param name="port">Port number</param>
        public WsServer(IPAddress address, int port) : base(address, port) { WebSocket = new WebSocket(this); }
        /// <summary>
        /// Initialize WebSocket server with a given IP address and port number
        /// </summary>
        /// <param name="address">IP address</param>
        /// <param name="port">Port number</param>
        public WsServer(string address, int port) : base(address, port) { WebSocket = new WebSocket(this); }
        /// <summary>
        /// Initialize WebSocket server with a given DNS endpoint
        /// </summary>
        /// <param name="endpoint">DNS endpoint</param>
        public WsServer(DnsEndPoint endpoint) : base(endpoint) { WebSocket = new WebSocket(this); }
        /// <summary>
        /// Initialize WebSocket server with a given IP endpoint
        /// </summary>
        /// <param name="endpoint">IP endpoint</param>
        public WsServer(IPEndPoint endpoint) : base(endpoint) { WebSocket = new WebSocket(this); }

        public virtual bool CloseAll(int status)
        {
            lock (WebSocket.WsSendLock)
            {
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_CLOSE, false, null, 0, 0, status);
                if (!Multicast(WebSocket.WsSendBuffer.ToArray()))
                    return false;

                return base.DisconnectAll();
            }
        }

        public override bool Multicast(byte[] buffer, long offset, long size)
        {
            if (!IsStarted)
                return false;

            if (size == 0)
                return true;

            // Multicast data to all WebSocket sessions
            foreach (var session in Sessions.Values)
            {
                if (session is WsSession wsSession)
                {
                    if (wsSession.WebSocket.WsHandshaked)
                        wsSession.SendAsync(buffer, offset, size);
                }
            }

            return true;
        }

        #region WebSocket multicast text methods

        public bool MulticastText(byte[] buffer, long offset, long size)
        {
            lock (WebSocket.WsSendLock)
            {
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_TEXT, false, buffer, offset, size);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        public bool MulticastText(string text)
        {
            lock (WebSocket.WsSendLock)
            {
                var data = Encoding.UTF8.GetBytes(text);
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_TEXT, false, data, 0, data.Length);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        #endregion

        #region WebSocket multicast binary methods

        public bool MulticastBinary(byte[] buffer, long offset, long size)
        {
            lock (WebSocket.WsSendLock)
            {
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_BINARY, false, buffer, offset, size);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        public bool MulticastBinary(string text)
        {
            lock (WebSocket.WsSendLock)
            {
                var data = Encoding.UTF8.GetBytes(text);
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_BINARY, false, data, 0, data.Length);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        #endregion

        #region WebSocket multicast ping methods

        public bool SendPing(byte[] buffer, long offset, long size)
        {
            lock (WebSocket.WsSendLock)
            {
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PING, false, buffer, offset, size);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        public bool SendPing(string text)
        {
            lock (WebSocket.WsSendLock)
            {
                var data = Encoding.UTF8.GetBytes(text);
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PING, false, data, 0, data.Length);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        #endregion

        #region WebSocket multicast pong methods

        public bool SendPong(byte[] buffer, long offset, long size)
        {
            lock (WebSocket.WsSendLock)
            {
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PONG, false, buffer, offset, size);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        public bool SendPong(string text)
        {
            lock (WebSocket.WsSendLock)
            {
                var data = Encoding.UTF8.GetBytes(text);
                WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PONG, false, data, 0, data.Length);
                return Multicast(WebSocket.WsSendBuffer.ToArray());
            }
        }

        #endregion

        protected override TcpSession CreateSession() { return new WsSession(this); }
    }
}
