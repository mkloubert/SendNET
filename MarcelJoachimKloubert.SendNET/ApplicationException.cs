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

using System;
using System.Runtime.Serialization;
using System.Security;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// An application exception.
    /// </summary>
    public class ApplicationException : Exception
    {
        #region Constructors (4)

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException" /> class.
        /// </summary>
        /// <param name="code">The value for the <see cref="ApplicationException.Code" /> property.</param>
        public ApplicationException(int code = 0)
            : base()
        {
            this.Code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException" /> class.
        /// </summary>
        /// <param name="message">The value for the <see cref="Exception.Message" /> property.</param>
        /// <param name="code">The value for the <see cref="ApplicationException.Code" /> property.</param>
        public ApplicationException(string message, int code = 0)
            : base(message)
        {
            this.Code = code;
        }

        /// <summary>
        /// <see cref="Exception.Exception(SerializationInfo, StreamingContext)" />
        /// </summary>
        [SecuritySafeCritical]
        protected ApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException" /> class.
        /// </summary>
        /// <param name="message">The value for the <see cref="Exception.Message" /> property.</param>
        /// <param name="innerException">The value for the <see cref="Exception.InnerException" /> property.</param>
        /// <param name="code">The value for the <see cref="ApplicationException.Code" /> property.</param>
        public ApplicationException(string message, Exception innerException, int code = 0)
            : base(message, innerException)
        {
            this.Code = code;
        }

        #endregion Constructors (4)

        #region Properties (1)

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public int Code
        {
            get;
            private set;
        }

        #endregion Properties (1)
    }
}