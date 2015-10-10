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

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// A simple <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> object.
    /// </summary>
    /// <typeparam name="T">Object / value type.</typeparam>
    public class ObjectComparer<T> : IComparer<T>, IEqualityComparer<T>
    {
        #region Fields (4)

        private readonly Func<T, T, int> _COMPARE;
        private readonly Func<T, T, bool> _EQUALS;
        private readonly Func<T, int> _GET_HASHCODE;
        private static ObjectComparer<T> _default;

        #endregion Fields (4)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectComparer{T}" /> class.
        /// </summary>
        /// <param name="equalsFunc">The logic for the <see cref="ObjectComparer{T}.Equals(T, T)" /> method.</param>
        /// <param name="compareFunc">The logic for the <see cref="ObjectComparer{T}.Compare(T, T)" /> method.</param>
        /// <param name="getHashCodeFunc">The logic for the <see cref="ObjectComparer{T}.GetHashCode(T)" /> method.</param>
        public ObjectComparer(Func<T, T, bool> equalsFunc = null,
                              Func<T, T, int> compareFunc = null,
                              Func<T, int> getHashCodeFunc = null)
        {
            this._EQUALS = equalsFunc ?? this.DefaultEquals;
            this._COMPARE = compareFunc ?? this.DefaultCompare;
            this._GET_HASHCODE = getHashCodeFunc ?? this.DefaultGetHashCode;
        }

        #endregion Constructors (1)

        #region Properties

        /// <summary>
        /// Gets the default instance of that class.
        /// </summary>
        public static ObjectComparer<T> Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ObjectComparer<T>();
                }

                return _default;
            }
        }

        #endregion Properties

        #region Methods (6)

        /// <summary>
        /// The default logic for the <see cref="ObjectComparer{T}.Compare(T, T)" /> method.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>The sort value.</returns>
        protected virtual int DefaultCompare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }

        /// <summary>
        /// The default logic for the <see cref="ObjectComparer{T}.Equals(T, T)" /> method.
        /// </summary>
        /// <param name="x">The left value.</param>
        /// <param name="y">The right value.</param>
        /// <returns>Are equal or not.</returns>
        protected virtual bool DefaultEquals(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        /// <summary>
        /// The default logic for the <see cref="ObjectComparer{T}.GetHashCode(T)" /> method.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The hash code.</returns>
        protected virtual int DefaultGetHashCode(T obj)
        {
            return EqualityComparer<T>.Default.GetHashCode(obj);
        }

        /// <summary>
        /// <see cref="IComparer{T}.Compare(T, T)" />
        /// </summary>
        public int Compare(T x, T y)
        {
            return this._COMPARE(x, y);
        }

        /// <summary>
        /// <see cref="IEqualityComparer{T}.Equals(T, T)" />
        /// </summary>
        public bool Equals(T x, T y)
        {
            return this._EQUALS(x, y);
        }

        /// <summary>
        /// <see cref="IEqualityComparer{T}.GetHashCode(T)" />
        /// </summary>
        public int GetHashCode(T obj)
        {
            return this._GET_HASHCODE(obj);
        }

        #endregion Methods (6)
    }
}