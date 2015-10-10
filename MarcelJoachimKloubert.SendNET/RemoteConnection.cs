/**********************************************************************************************************************
 * Send.NET (https://github.com/mkloubert/SendNET)                                                                    *
 *                                                                                                                    *
 * Copyright (c) 2015, Marcel Joachim Kloubert <marcel.kloubert@gmx.net>                                              *
 * All rights reserved.                                                                                               *
 *                                                                                                                    *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the   *
 * following conditions are met:                                                                                      *
 *                                                                                                                    *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the          *
 *    following disclaimer.                                                                                           *
 *                                                                                                                    *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the       *
 *    following disclaimer in the documentation and/or other materials provided with the distribution.                *
 *                                                                                                                    *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote    *
 *    products derived from this software without specific prior written permission.                                  *
 *                                                                                                                    *
 *                                                                                                                    *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, *
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  *
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, *
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR    *
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  *
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE   *
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                                           *
 *                                                                                                                    *
 **********************************************************************************************************************/

using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// A remote connection.
    /// </summary>
    public class RemoteConnection : DisposableBase, IRemoteConnection
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConnection" /> class.
        /// </summary>
        /// <param name="appContext">The value for the <see cref="ApplicationObject.Application" /> property.</param>
        /// <param name="socket">The underlying socket connection.</param>
        /// <param name="ownsSocket">
        /// Own <paramref name="socket" /> or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> and/or <paramref name="socket" /> is <see langword="null" />.
        /// </exception>
        public RemoteConnection(IAppContext appContext, Socket socket, bool ownsSocket = true)
            : base(appContext: appContext)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            this.Address = socket.RemoteEndPoint;
            this.Socket = socket;
            this.Stream = new NetworkStream(socket, ownsSocket);
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// <see cref="IRemoteConnection.Address" />
        /// </summary>
        public EndPoint Address
        {
            get;
            private set;
        }

        /// <summary>
        /// <see cref="IRemoteConnection.Socket" />
        /// </summary>
        public Socket Socket
        {
            get;
            private set;
        }

        /// <summary>
        /// <see cref="IRemoteConnection.Stream" />
        /// </summary>
        public NetworkStream Stream
        {
            get;
            private set;
        }

        #endregion Properties (3)

        #region Methods (6)

        /// <summary>
        /// <see cref="IRemoteConnection.Close()" />
        /// </summary>
        public void Close()
        {
            lock (this._SYNC)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                try
                {
                    using (this.Stream)
                    {
                        if (this.Socket.Connected)
                        {
                            this.Stream.Close();
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// Opens a new connection to a remote socket.
        /// </summary>
        /// <param name="appContext">The underlying application context.</param>
        /// <param name="hostAddress">The remote IP/address.</param>
        /// <param name="port">The remote port.</param>
        /// <returns>The new connection.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="port" /> is invalid.
        /// </exception>
        /// <exception cref="SocketException">
        /// Connecting failed.
        /// </exception>
        public static RemoteConnection ConnectTo(IAppContext appContext, IEnumerable<char> hostAddress, int port)
        {
            var host = hostAddress.AsString();
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "127.0.0.1";
            }
            else
            {
                host = host.Trim();
            }

            return ConnectTo(appContext,
                             Dns.GetHostEntry(host).AddressList.First(),
                             port);
        }

        /// <summary>
        /// Opens a new connection to a remote socket.
        /// </summary>
        /// <param name="appContext">The underlying application context.</param>
        /// <param name="address">The remote address.</param>
        /// <param name="port">The remote port.</param>
        /// <returns>The new connection.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> and/or <paramref name="address" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="port" /> is invalid.
        /// </exception>
        /// <exception cref="SocketException">
        /// Connecting failed.
        /// </exception>
        public static RemoteConnection ConnectTo(IAppContext appContext, IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            return ConnectTo(appContext,
                             new IPEndPoint(address, port));
        }

        /// <summary>
        /// Opens a new connection to a remote socket.
        /// </summary>
        /// <param name="appContext">The underlying application context.</param>
        /// <param name="remoteEP">The remote endpoint.</param>
        /// <returns>The new connection.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> and/or <paramref name="remoteEP" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="SocketException">
        /// Connecting failed.
        /// </exception>
        public static RemoteConnection ConnectTo(IAppContext appContext, IPEndPoint remoteEP)
        {
            if (appContext == null)
            {
                throw new ArgumentNullException("appContext");
            }

            if (remoteEP == null)
            {
                throw new ArgumentNullException("endPoint");
            }

            var client = new TcpClient();
            client.Connect(remoteEP);

            return new RemoteConnection(appContext,
                                        client.Client,
                                        true);
        }

        /// <summary>
        /// <see cref="DisposableBase.OnDispose(bool, ref bool)" />
        /// </summary>
        protected override void OnDispose(bool disposing, ref bool isDisposed)
        {
            if (disposing)
            {
                try
                {
                    this.Stream.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, true);
                }
            }
            else
            {
                try
                {
                    this.Close();
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// <see cref="IRemoteConnection.IsValid(ConnectionValidator)" />
        /// </summary>
        public bool IsValid(ConnectionValidator validator)
        {
            if (validator == null)
            {
                return true;
            }

            return validator(this);
        }

        #endregion Methods (6)
    }
}