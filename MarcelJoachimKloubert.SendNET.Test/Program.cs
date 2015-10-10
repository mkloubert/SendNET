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

using MarcelJoachimKloubert.SendNET.Client;
using MarcelJoachimKloubert.SendNET.Server;
using System;

namespace MarcelJoachimKloubert.SendNET.Test
{
    internal static class Program
    {
        #region Methods (2)

        private static void Main(string[] args)
        {
            try
            {
                var serverApp = new AppContext();
                {
                    var serverSettings = new AppSettings(serverApp);

                    serverApp.Settings = serverSettings;
                }

                var clientApp = new AppContext();
                {
                    var clientSettings = new AppSettings(clientApp);

                    clientApp.Settings = clientSettings;
                }

                using (var host = new ServerHost(serverApp))
                {
                    host.Disposing += (sender, e) =>
                        {
                            Console.Write("Disposing... ");
                        };
                    host.Disposed += (sender, e) =>
                        {
                            Console.WriteLine("[OK]");
                        };

                    host.Starting += (sender, e) =>
                        {
                            Console.Write("Starting... ");
                        };
                    host.Started += (sender, e) =>
                        {
                            Console.WriteLine("[OK]");
                        };

                    host.Stopping += (sender, e) =>
                        {
                            Console.Write("Stopping... ");
                        };
                    host.Stopped += (sender, e) =>
                        {
                            Console.WriteLine("[OK]");
                        };

                    host.Start();

                    var client = ClientConnection.Open(clientApp.Settings);
                    client.SayHello();

                    WaitForEnter();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FATAL ERROR!!!]: {0}",
                                  ex.GetBaseException());
            }

            WaitForEnter();
        }

        private static void WaitForEnter()
        {
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("===== ENTER =====");
            Console.ReadLine();
        }

        #endregion Methods (2)
    }
}