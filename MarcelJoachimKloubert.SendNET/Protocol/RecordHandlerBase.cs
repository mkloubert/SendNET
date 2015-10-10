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
using System;

namespace MarcelJoachimKloubert.SendNET.Protocol
{
    /// <summary>
    /// A basic record handler.
    /// </summary>
    /// <typeparam name="TRecord">Type of the underlying record.</typeparam>
    public abstract class RecordHandlerBase<TRecord> : DisposableBase
        where TRecord : IRecord
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordHandlerBase{TRecord}" /> class.
        /// </summary>
        /// <param name="appContext">The value for the <see cref="ApplicationObject.Application" /> property.</param>
        /// <param name="record">The value for the <see cref="RecordHandlerBase{TRecord}.Record" /> property.</param>
        /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> and/or <paramref name="record" /> is <see langword="null" />.
        /// </exception>
        protected RecordHandlerBase(IAppContext appContext, TRecord record, object sync = null)
            : base(appContext: appContext,
                   sync: sync)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            this.Record = record;
        }

        #endregion Constructors (1)

        #region Properties (1)

        /// <summary>
        /// Gets the underlying record.
        /// </summary>
        public TRecord Record
        {
            get;
            private set;
        }

        #endregion Properties (1)

        #region Methods (2)

        /// <summary>
        /// Handles the underlying record.
        /// </summary>
        public void HandleNext()
        {
            lock (this._SYNC)
            {
                this.ThrowIfDisposed();

                try
                {
                    this.OnHandleNext();
                }
                catch (Exception ex)
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// The logic for the <see cref="RecordHandlerBase{TRecord}.HandleNext()" /> method.
        /// </summary>
        protected abstract void OnHandleNext();

        #endregion Methods (2)
    }
}