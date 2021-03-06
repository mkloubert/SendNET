﻿/**********************************************************************************************************************
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
using System.Net;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// Simple application settings.
    /// </summary>
    public class AppSettings : ApplicationObject, IAppSettings
    {
        #region Fields (1)

        /// <summary>
        /// Stores the default port.
        /// </summary>
        public const int DEFAULT_PORT = 5979;

        #endregion Fields (1)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings" /> class.
        /// </summary>
        /// <param name="appContext">The value for the <see cref="ApplicationObject.Application" /> property.</param>
        /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        public AppSettings(IAppContext appContext, object sync = null)
            : base(appContext: appContext,
                    sync: sync)
        {
            this.IP = null;
        }

        #endregion Constructors (1)

        #region Properties (4)

        /// <summary>
        /// <see cref="IAppSettings.Address" />
        /// </summary>
        public IPAddress Address
        {
            get { return this.Get(() => this.Address); }

            set { this.Set(() => this.Address, value); }
        }

        /// <summary>
        /// <see cref="IAppSettings.ConnectionValidator" />
        /// </summary>
        public ConnectionValidator ConnectionValidator
        {
            get { return this.Get(() => this.ConnectionValidator); }

            set { this.Set(() => this.ConnectionValidator, value); }
        }

        /// <summary>
        /// Sets the values for <see cref="AppSettings.Address" /> and <see cref="AppSettings.Port" /> properties.
        /// </summary>
        public IPEndPoint IP
        {
            set
            {
                if (value != null)
                {
                    this.Address = value.Address;
                    this.Port = value.Port;
                }
                else
                {
                    this.Address = null;
                    this.Port = DEFAULT_PORT;
                }
            }
        }

        /// <summary>
        /// <see cref="IAppSettings.Port" />
        /// </summary>
        public int Port
        {
            get { return this.Get(() => this.Port); }

            set
            {
                if ((value < IPEndPoint.MinPort) || (value > IPEndPoint.MaxPort))
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          string.Format("Allowed values are between {0} and {1}!",
                                                                        IPEndPoint.MinPort, IPEndPoint.MaxPort));
                }

                this.Set(() => this.Port, value);
            }
        }

        #endregion Properties (4)
    }
}