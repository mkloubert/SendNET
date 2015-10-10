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
using MarcelJoachimKloubert.SendNET.Cryptography;
using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;

namespace MarcelJoachimKloubert.SendNET.Protocol
{
    /// <summary>
    /// A known record.
    /// </summary>
    public abstract class RecordBase : UnknownRecord
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownRecord" /> class.
        /// </summary>
        /// <param name="knownType">The value for the <see cref="RecordBase.KnownType" /> property.</param>
        /// <param name="crypter">The value for the <see cref="UnknownRecord.Crypter" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="crypter" /> is <see langword="null" />.
        /// </exception>
        protected RecordBase(RecordType knownType, ICrypter crypter)
            : base(crypter: crypter)
        {
            this.KnownType = knownType;
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// <see cref="UnknownRecord.Content" />
        /// </summary>
        public sealed override byte[] Content
        {
            get { return base.Content; }

            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the known record type.
        /// </summary>
        public RecordType KnownType
        {
            get { return this.Get(() => this.KnownType); }

            private set { this.Set(() => this.KnownType, value); }
        }

        /// <summary>
        /// <see cref="UnknownRecord.Type" />
        /// </summary>
        [ReceiveNotificationFrom("KnownType")]
        public sealed override ushort Type
        {
            get { return (ushort)this.Get(() => this.KnownType); }

            set { throw new NotSupportedException(); }
        }

        #endregion Properties (3)

        #region Methods (3)

        /// <summary>
        /// <see cref="UnknownRecord.OnParseContent(byte[], ref bool)" />
        /// </summary>
        protected override void OnParseContent(byte[] newContent, ref bool success)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the value for the <see cref="RecordBase.Content" /> property.
        /// </summary>
        /// <param name="newData">The new data.</param>
        /// <exception cref="ArgumentOutOfRangeException">The new data is too big.</exception>
        protected void SetContentProperty(IEnumerable<byte> newData)
        {
            base.Content = newData.AsArray();
        }

        /// <summary>
        /// Updates the <see cref="RecordBase.Content" /> property.
        /// </summary>
        /// <param name="args">The arguments from the changed property.</param>
        protected abstract void UpdateContent(IReceiveValueFromArgs args);

        #endregion Methods (3)
    }
}