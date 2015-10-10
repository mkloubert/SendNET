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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace MarcelJoachimKloubert.SendNET.Client
{
    /// <summary>
    /// A connection to a remote host.
    /// </summary>
    public partial class ClientConnection : DisposableBase
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection" /> class.
        /// </summary>
        /// <param name="settings">The value for the <see cref="ApplicationObject.Settings" /> property.</param>
        /// <param name="connection">The value for the <see cref="ClientConnection.Connection" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="connection" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// <paramref name="settings" /> is <see langword="null" />.
        /// </exception>
        protected ClientConnection(IAppSettings settings, IRemoteConnection connection)
            : base(appContext: settings.Application)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            this.Connection = connection;
        }

        #endregion Constructors (1)

        #region Properties (3)

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
        /// Gets the connection is open or not.
        /// </summary>
        [ReceiveNotificationFrom("Crypter")]
        public bool IsOpen
        {
            get { return this.Crypter != null; }
        }

        /// <summary>
        /// Gets the stream of <see cref="ClientConnection.Connection" />.
        /// </summary>
        public NetworkStream Stream
        {
            get { return this.Connection.Stream; }
        }

        #endregion Properties (3)

        #region Methods (7)

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            lock (this._SYNC)
            {
                try
                {
                    this.Connection.Close();
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// Makes a handshake.
        /// </summary>
        protected void MakeHandshake()
        {
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                var key = rsa.ExportCspBlob(false);

                // version
                this.Stream.WriteByte(1);

                // key size
                var keySize = (ushort)key.Length;
                this.Stream.Write(keySize.GetBytes(), 0, 2);

                // key
                this.Stream.Write(key, 0, key.Length);

                if (!this.WaitForHandshake(rsa))
                {
                    this.Close();
                    return;
                }
            }
        }

        /// <summary>
        /// <see cref="DisposableBase.OnDispose(bool, ref bool)" />
        /// </summary>
        protected override void OnDispose(bool disposing, ref bool isDisposed)
        {
            try
            {
                using (this.Connection)
                {
                    this.Connection.Close();
                }
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
        /// Opens a new connection from settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The new connection.</returns>
        /// <exception cref="InvalidConnectionException">
        /// Connection is not possible.
        /// </exception>
        public static ClientConnection Open(IAppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var addr = settings.Address ?? IPAddress.Loopback;
            var port = settings.Port;

            var connection = RemoteConnection.ConnectTo(settings.Application,
                                                        addr, port);

            if (!connection.IsValid(settings.ConnectionValidator))
            {
                try
                {
                    using (connection)
                    {
                        connection.Close();
                    }
                }
                finally
                {
                    throw new InvalidConnectionException(connection);
                }
            }

            var result = new ClientConnection(settings, connection);
            result.Start();

            return result;
        }

        /// <summary>
        /// Sends a &quot;hello&quot; to the server.
        /// </summary>
        public void SayHello()
        {
            lock (this._SYNC)
            {
                try
                {
                    var helloServer = new ClientHelloRecord(this.Crypter);
                    helloServer.SendTo(this.Connection);

                    foreach (var record in UnknownRecord.FromStream(this.Stream, this.Crypter))
                    {
                        if (record != null)
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// Starts the process.
        /// </summary>
        protected void Start()
        {
            lock (this._SYNC)
            {
                this.ThrowIfDisposed();

                try
                {
                    this.MakeHandshake();
                }
                catch (Exception ex)
                {
                    this.Close();

                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// Waits for a handshake from the server.
        /// </summary>
        /// <param name="rsa">The RSA crypter.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="rsa" /> is <see langword="null" />.
        /// </exception>
        protected bool WaitForHandshake(RSACryptoServiceProvider rsa)
        {
            if (rsa == null)
            {
                throw new ArgumentNullException("rsa");
            }

            byte[] buffer;

            buffer = this.Stream.WaitAndRead(2);
            if (buffer.Length != 2)
            {
                return false;
            }

            // content length
            var contentLength = buffer.ToUInt16().Value;
            if (contentLength < 1)
            {
                // no data
                return false;
            }

            buffer = this.Stream.WaitAndRead(contentLength);
            if (buffer.Length != contentLength)
            {
                return false;
            }

            var uncrypted = rsa.Decrypt(buffer, false);
            if (uncrypted.Length < 1)
            {
                return false;
            }

            CryptAlgorithm algo;
            if (!Enum.TryParse<CryptAlgorithm>(uncrypted[0].ToString(), out algo))
            {
                return false;
            }

            ICrypter crypter = null;

            switch (algo)
            {
                case CryptAlgorithm.UNKNOWN:
                    break;

                case CryptAlgorithm.Rijndael:
                    crypter = RijndaelCrypter.FromParameters(uncrypted.Skip(1));
                    break;

                default:
                    throw new NotImplementedException(algo.ToString());
            }

            if (crypter == null)
            {
                return false;
            }

            this.Crypter = crypter;
            return true;
        }

        #endregion Methods (7)
    }
}