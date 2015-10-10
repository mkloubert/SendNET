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
using MarcelJoachimKloubert.SendNET.Protocol;
using System;

namespace MarcelJoachimKloubert.SendNET.Client.Protocol
{
    /// <summary>
    /// A hello from client.
    /// </summary>
    [Record(RecordType.ClientHello)]
    public class ClientHelloRecord : RecordBase
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHelloRecord" /> class.
        /// </summary>
        /// <param name="crypter">The value for the <see cref="UnknownRecord.Crypter" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="crypter" /> is <see langword="null" />.
        /// </exception>
        public ClientHelloRecord(ICrypter crypter)
            : base(knownType: RecordType.ClientHello,
                   crypter: crypter)
        {
        }

        #endregion Constructors (1)

        #region Methods (2)

        /// <summary>
        /// <see cref="RecordBase.OnParseContent(byte[], ref bool)" />
        /// </summary>
        protected override void OnParseContent(byte[] newContent, ref bool success)
        {
            success = true;
        }

        /// <summary>
        /// <see cref="RecordBase.UpdateContent(IReceiveValueFromArgs)" />
        /// </summary>
        protected override void UpdateContent(IReceiveValueFromArgs args)
        {
        }

        #endregion Methods (2)
    }
}