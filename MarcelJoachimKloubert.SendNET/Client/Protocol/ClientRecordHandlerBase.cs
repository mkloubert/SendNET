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

using MarcelJoachimKloubert.SendNET.ComponentModel;
using MarcelJoachimKloubert.SendNET.Protocol;
using System;

namespace MarcelJoachimKloubert.SendNET.Client
{
    partial class ClientConnection
    {
        /// <summary>
        /// A basic record handler for a <see cref="ClientConnection" /> instance.
        /// </summary>
        /// <typeparam name="TRecord">Type of the underlying record.</typeparam>
        protected abstract class ClientRecordHandlerBase<TRecord> : RecordHandlerBase<TRecord>
            where TRecord : IRecord
        {
            #region Constructors (1)

            /// <summary>
            /// Initializes a new instance of the <see cref="ClientRecordHandlerBase{TRecord}" /> class.
            /// </summary>
            /// <param name="conn">The value for the <see cref="ClientRecordHandlerBase{TRecord}.ClientConnection" /> property.</param>
            /// <param name="record">The value for the <see cref="RecordHandlerBase{TRecord}.Record" /> property.</param>
            /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="record" /> is <see langword="null" />.
            /// </exception>
            /// <exception cref="NullReferenceException">
            /// <paramref name="conn" /> is <see langword="null" />.
            /// </exception>
            protected ClientRecordHandlerBase(ClientConnection conn, TRecord record, object sync = null)
                : base(appContext: conn.Application,
                       record: record,
                       sync: sync)
            {
                this.ClientConnection = conn;
            }

            #endregion Constructors (1)

            #region Properties (2)

            /// <summary>
            /// Gets the underlying client connection.
            /// </summary>
            public ClientConnection ClientConnection
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the underlying remote connection.
            /// </summary>
            public IRemoteConnection Connection
            {
                get { return this.ClientConnection.Connection; }
            }

            #endregion Properties (2)
        }
    }
}