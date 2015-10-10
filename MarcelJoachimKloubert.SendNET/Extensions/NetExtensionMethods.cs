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
using System.Linq;
using System.Net.Sockets;

namespace MarcelJoachimKloubert.SendNET.Extensions
{
    /// <summary>
    /// Extension methods for network operations.
    /// </summary>
    static partial class SendNETExtensionMethods
    {
        #region Methods (3)

        /// <summary>
        /// Waits for data and reads them.
        /// </summary>
        /// <param name="stream">The underyling stream.</param>
        /// <param name="size">The expected size in bytes.</param>
        /// <returns>The read data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size" /> is less than 0.
        /// </exception>
        public static byte[] WaitAndRead(this NetworkStream stream, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("expectedSize", size,
                                                      "Is less than 0!");
            }

            var result = new byte[size];
            var bytesRead = WaitForData(stream).Read(result, 0, result.Length);

            if (bytesRead != result.Length)
            {
                result = AsArray(result.Take(bytesRead));
            }

            return result;
        }

        /// <summary>
        /// Waits for data and reads a byte.
        /// </summary>
        /// <param name="stream">The underyling stream.</param>
        /// <returns>The read byte or <see langword="null" /> if no data is available.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream" /> is <see langword="null" />.
        /// </exception>
        public static byte? WaitAndReadByte(this NetworkStream stream)
        {
            var result = WaitForData(stream).ReadByte();

            return result >= 0 ? (byte)result : (byte?)null;
        }

        /// <summary>
        /// Waits for data.
        /// </summary>
        /// <param name="stream">The underyling stream.</param>
        /// <param name="predicate">
        /// An optional predicate that returns <see langword="true" /> for continue waiting; otherwise <see langword="false" />.
        /// </param>
        /// <returns>The instance of <paramref name="stream" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream" /> is <see langword="null" />.
        /// </exception>
        public static TStream WaitForData<TStream>(this TStream stream, Func<TStream, bool> predicate = null)
            where TStream : global::System.Net.Sockets.NetworkStream
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            predicate = predicate ?? new Func<TStream, bool>((ns) => true);

            while (!stream.DataAvailable)
            {
                if (!predicate(stream))
                {
                    break;
                }
            }

            return stream;
        }

        #endregion Methods (3)
    }
}