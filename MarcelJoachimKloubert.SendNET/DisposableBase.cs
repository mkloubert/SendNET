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

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// A basic disposable object.
    /// </summary>
    public abstract class DisposableBase : ApplicationObject, IDisposable
    {
        #region Constructors (2)

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableBase" /> class.
        /// </summary>
        /// <param name="appContext">The value for the <see cref="ApplicationObject.Application" /> property.</param>
        /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> is <see langword="null" />.
        /// </exception>
        protected DisposableBase(IAppContext appContext, object sync = null)
            : base(appContext: appContext,
                   sync: sync)
        {
        }

        /// <summary>
        /// Frees the <see cref="DisposableBase" /> object.
        /// </summary>
        ~DisposableBase()
        {
            this.Dispose(false);
        }

        #endregion Constructors (2)

        #region Events (2)

        /// <summary>
        /// Is raised when object begins disposing itself.
        /// </summary>
        public event EventHandler Disposing;

        /// <summary>
        /// Is raised when the object has been disposed.
        /// </summary>
        public event EventHandler Disposed;

        #endregion Events (2)

        #region Properties (1)

        /// <summary>
        /// Gets if the object has been disposed (<see langword="true" />) or not (<see langword="false" />).
        /// </summary>
        public bool IsDisposed
        {
            get { return this.Get(() => this.IsDisposed); }

            private set { this.Set(() => this.IsDisposed, value); }
        }

        #endregion Properties (1)

        #region Methods (4)

        /// <summary>
        /// <see cref="IDisposable.Dispose()" />
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (this._SYNC)
            {
                if (disposing && this.IsDisposed)
                {
                    return;
                }

                try
                {
                    if (disposing)
                    {
                        this.RaiseEventHandler(this.Disposing);
                    }

                    var isDisposed = disposing ? true : this.IsDisposed;
                    this.OnDispose(disposing, ref isDisposed);

                    this.IsDisposed = isDisposed;
                    if (this.IsDisposed)
                    {
                        this.RaiseEventHandler(this.Disposed);
                    }
                }
                catch (Exception ex)
                {
                    if (disposing)
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// The logic for the <see cref="DisposableBase.Dispose()" /> method or the destructor.
        /// </summary>
        /// <param name="disposing">
        /// <see cref="DisposableBase.Dispose()" /> method was invoked (<see langword="true" />)
        /// or the destructor (<see langword="false" />).
        /// </param>
        /// <param name="isDisposed">
        /// The new value for <see cref="DisposableBase.IsDisposed" /> property.
        /// </param>
        protected abstract void OnDispose(bool disposing, ref bool isDisposed);

        /// <summary>
        /// Throws an exception if that object has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        #endregion Methods (4)
    }
}