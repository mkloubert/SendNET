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
using System.Reflection;

namespace MarcelJoachimKloubert.SendNET.ComponentModel
{
    partial class NotifiableBase
    {
        private class ReceiveValueFromArgs : IReceiveValueFromArgs
        {
            #region Constructor (1)

            internal ReceiveValueFromArgs(NotifiableBase sender)
            {
                this.Sender = sender;
            }

            #endregion Constructor (1)

            #region Properties (8)

            public object NewValue
            {
                get;
                internal set;
            }

            public object OldValue
            {
                get;
                internal set;
            }

            public NotifiableBase Sender
            {
                get;
                private set;
            }

            object IReceiveValueFromArgs.Sender
            {
                get { return this.Sender; }
            }

            public string SenderName
            {
                get;
                internal set;
            }

            public MemberTypes SenderType
            {
                get;
                internal set;
            }

            int IReceiveValueFromArgs.SenderType
            {
                get { return (int)this.SenderType; }
            }

            public Type TargetType
            {
                get;
                internal set;
            }

            #endregion Properties (8)

            #region Methods (3)

            public TTarget GetNewValue<TTarget>()
            {
                return this.Sender.ConvertTo<TTarget>(this.NewValue);
            }

            public TTarget GetOldValue<TTarget>()
            {
                return this.Sender.ConvertTo<TTarget>(this.OldValue);
            }

            public TTarget GetSender<TTarget>()
            {
                return this.Sender.ConvertTo<TTarget>(this.Sender);
            }

            #endregion Methods (3)
        }
    }
}