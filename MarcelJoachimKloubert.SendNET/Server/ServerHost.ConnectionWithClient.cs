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

using MarcelJoachimKloubert.SendNET.Client.Protocol;
using MarcelJoachimKloubert.SendNET.ComponentModel;
using MarcelJoachimKloubert.SendNET.Cryptography;
using MarcelJoachimKloubert.SendNET.Extensions;
using MarcelJoachimKloubert.SendNET.Protocol;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace MarcelJoachimKloubert.SendNET.Server
{
    partial class ServerHost
    {
        /// <summary>
        /// A connection with a client.
        /// </summary>
        protected class ConnectionWithClient : DisposableBase, IClientConnection
        {
            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionWithClient" /> class.
            /// </summary>
            /// <param name="host">The value for the <see cref="ConnectionWithClient.Host" /> property.</param>
            /// <param name="conn">The value for the <see cref="ConnectionWithClient.Connection" /> property.</param>
            /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
            public ConnectionWithClient(ServerHost host, IRemoteConnection conn, object sync = null)
                : base(appContext: host.Application,
                       sync: sync)
            {
                if (conn == null)
                {
                    throw new ArgumentNullException("conn");
                }

                this.Host = host;
                this.Connection = conn;
            }

            #endregion Constructors (1)

            #region Events (1)

            /// <summary>
            /// Is raised when the connection was closed.
            /// </summary>
            public event EventHandler Closed;

            #endregion Events (1)

            #region Properties (6)

            EndPoint IRemoteConnection.Address
            {
                get { return this.Connection.Address; }
            }

            /// <summary>
            /// Gets the underlying connection.
            /// </summary>
            public IRemoteConnection Connection
            {
                get { return this.Get(() => this.Connection); }

                protected set { this.Set(() => this.Connection, value); }
            }

            /// <summary>
            /// Gets the current crypter.
            /// </summary>
            public ICrypter Crypter
            {
                get { return this.Get(() => this.Crypter); }

                protected set { this.Set(() => this.Crypter, value); }
            }

            /// <summary>
            /// Gets the underlying host.
            /// </summary>
            public ServerHost Host
            {
                get;
                private set;
            }

            Socket IRemoteConnection.Socket
            {
                get { return this.Connection.Socket; }
            }

            /// <summary>
            /// <see cref="IRemoteConnection.Stream" />
            /// </summary>
            public NetworkStream Stream
            {
                get { return this.Connection.Stream; }
            }

            #endregion Properties (6)

            #region Methods (4)

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
                        this.Connection.Close();

                        if (!this.Connection.Socket.Connected)
                        {
                            this.RaiseEventHandler(this.Closed);

                            this.Connection = null;
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

            bool IRemoteConnection.IsValid(ConnectionValidator validator)
            {
                return this.Connection.IsValid(validator);
            }

            /// <summary>
            /// <see cref="DisposableBase.OnDispose(bool, ref bool)" />
            /// </summary>
            protected override void OnDispose(bool disposing, ref bool isDisposed)
            {
                try
                {
                    this.Close();
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    if (disposing)
                    {
                        this.RaiseError(ex, true);
                    }
                }
            }

            /// <summary>
            /// Starts the process.
            /// </summary>
            public void Start()
            {
                lock (this._SYNC)
                {
                    this.ThrowIfDisposed();

                    try
                    {
                        if (!this.WaitForHandshake())
                        {
                            this.Close();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Close();

                        this.RaiseError(ex, true);
                    }
                }
            }

            /// <summary>
            /// Waits for a handshake from client.
            /// </summary>
            protected bool WaitForHandshake()
            {
                byte[] buffer;

                buffer = this.Stream.WaitAndRead(1);
                if (buffer.Length != 1)
                {
                    return false;
                }

                var version = buffer[0];
                if (version != 1)
                {
                    // not supported
                    return false;
                }

                buffer = this.Stream.WaitAndRead(2);
                if (buffer.Length != 2)
                {
                    return false;
                }

                var keySize = buffer.ToUInt16().Value;
                if (keySize < 1)
                {
                    // no key
                    return false;
                }

                var key = this.Stream.WaitAndRead(keySize);
                if (key.Length != keySize)
                {
                    return false;
                }

                RijndaelCrypter crypter;

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportCspBlob(key);

                    crypter = new RijndaelCrypter();

                    byte[] content;
                    using (var temp = new MemoryStream())
                    {
                        // algorithm
                        var algo = CryptAlgorithm.Rijndael;
                        temp.WriteByte((byte)algo);

                        // parameters
                        var @params = crypter.ExportParameters();
                        temp.Write(@params, 0, @params.Length);

                        content = rsa.Encrypt(temp.ToArray(), false);
                    }

                    // content length
                    var contentLength = (ushort)content.Length;
                    this.Stream.Write(contentLength.GetBytes(), 0, 2);

                    // content
                    this.Stream.Write(content, 0, content.Length);
                }

                this.Crypter = crypter;

                return true;
            }

            /// <summary>
            /// Starts waiting for instructions.
            /// </summary>
            [ReceiveValueFrom("Crypter")]
            protected void WaitForInstructions()
            {
                while (this.Connection != null)
                {
                    foreach (var record in UnknownRecord.FromStream(this.Stream, this.Crypter))
                    {
                        if (record is ClientHelloRecord)
                        {
                            var helloFromClient = record as ClientHelloRecord;
                        }
                    }
                }
            }

            #endregion Methods (4)
        }
    }
}