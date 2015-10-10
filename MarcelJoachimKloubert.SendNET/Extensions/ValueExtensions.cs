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

namespace MarcelJoachimKloubert.SendNET.Extensions
{
    /// <summary>
    /// Value / object based extension methods.
    /// </summary>
    static partial class SendNETExtensionMethods
    {
        #region Methods (5)

        /// <summary>
        /// Checks if a nullable <see cref="bool" /> has the value <see langword="false" />.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <paramref name="value" /> is <see langword="false" /> or not.
        /// </returns>
        public static bool IsFalse(this bool? value)
        {
            return false == value;
        }

        /// <summary>
        /// Returns an object as string.
        /// </summary>
        /// <param name="obj">The input value.</param>
        /// <param name="dbNullAsNull">
        /// Returns <see cref="DBNull" /> as <see langword="null" /> reference or not.
        /// </param>
        /// <returns>The output value.</returns>
        public static string AsString(this object obj, bool dbNullAsNull = true)
        {
            if (obj is string)
            {
                return (string)obj;
            }

            if (dbNullAsNull)
            {
                if (DBNull.Value.Equals(dbNullAsNull))
                {
                    obj = null;
                }
            }

            if (obj == null)
            {
                return null;
            }

            if (obj is IEnumerable<char>)
            {
                return new string(AsArray(obj as IEnumerable<char>));
            }

            return obj.ToString();
        }

        /// <summary>
        /// Checks if an object is <see langword="null" /> or <see cref="DBNull" />.
        /// </summary>
        /// <typeparam name="TClass">Type of the object.</typeparam>
        /// <param name="obj">The object to check.</param>
        /// <returns>Is <see langword="null" /> or <see cref="DBNull"/>; otherwise <see langword="false" /></returns>
        public static bool IsNull<TClass>(this TClass obj)
             where TClass : class
        {
            return (null == obj) ||
                   DBNull.Value.Equals(obj);
        }

        /// <summary>
        /// Checks if a value is <see langword="null" />.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <returns>Is <see langword="null" /> or not.</returns>
        public static bool IsNull<TValue>(this Nullable<TValue> value)
            where TValue : struct
        {
            return !value.HasValue;
        }

        /// <summary>
        /// Checks if a nullable <see cref="bool" /> has the value <see langword="true" />.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <paramref name="value" /> is <see langword="true" /> or not.
        /// </returns>
        public static bool IsTrue(this bool? value)
        {
            return true == value;
        }

        #endregion Methods (5)
    }
}