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
using System.Collections.Generic;
using System.Linq;

namespace MarcelJoachimKloubert.SendNET.Extensions
{
    /// <summary>
    /// Extension methods for bit / byte operations.
    /// </summary>
    static partial class SendNETExtensionMethods
    {
        #region Methods (5)

        /// <summary>
        /// Returns the binary data for a <see cref="ushort" /> value.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The output value.</returns>
        public static byte[] GetBytes(this ushort value)
        {
            return AsArray(UpdateByteOrder(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// Returns the binary data for a <see cref="uint" /> value.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The output value.</returns>
        public static byte[] GetBytes(this uint value)
        {
            return AsArray(UpdateByteOrder(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// Converts binary data to a <see cref="ushort" /> value.
        /// </summary>
        /// <param name="bytes">The input data.</param>
        /// <returns>The output value.</returns>
        public static ushort? ToUInt16(this IEnumerable<byte> bytes)
        {
            bytes = UpdateByteOrder(bytes);

            if (bytes != null)
            {
                return BitConverter.ToUInt16(AsArray(bytes.Take(2)), 0);
            }

            return null;
        }

        /// <summary>
        /// Converts binary data to a <see cref="uint" /> value.
        /// </summary>
        /// <param name="bytes">The input data.</param>
        /// <returns>The output value.</returns>
        public static uint? ToUInt32(this IEnumerable<byte> bytes)
        {
            bytes = UpdateByteOrder(bytes);

            if (bytes != null)
            {
                return BitConverter.ToUInt32(AsArray(bytes.Take(4)), 0);
            }

            return null;
        }

        /// <summary>
        /// Updates the order binary data depending on the system settings in <see cref="BitConverter.IsLittleEndian" />.
        /// </summary>
        /// <param name="bytes">The input data.</param>
        /// <returns>The output data.</returns>
        public static IEnumerable<byte> UpdateByteOrder(this IEnumerable<byte> bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse();
            }

            return bytes;
        }

        #endregion Methods (5)
    }
}