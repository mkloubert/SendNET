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

using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.SendNET.Server
{
    /// <summary>
    /// A host.
    /// </summary>
    public partial class ServerHost : DisposableBase
    {
        #region Fields (2)

        private readonly IList<ConnectionWithClient> _CONNECTIONS;

        /// <summary>
        /// The current listener.
        /// </summary>
        protected TcpListener _listener;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerHost" /> class.
        /// </summary>
        /// <param name="appContext">The value for the <see cref="ApplicationObject.Application" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appContext" /> is <see langword="null" />.
        /// </exception>
        public ServerHost(IAppContext appContext)
            : base(appContext: appContext)
        {
            this._CONNECTIONS = this.CreateConnectionList() ?? new List<ConnectionWithClient>();
        }

        #endregion Constructors (1)

        #region Properties (1)

        /// <summary>
        /// Gets if the host is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get { return this.Get(() => this.IsRunning); }

            private set { this.Set(() => this.IsRunning, value); }
        }

        #endregion Properties (1)

        #region Events (6)

        /// <summary>
        /// Is raised when a connection with a remote client has been closed.
        /// </summary>
        public event EventHandler<ClientConnectionEventArgs> Closed;

        /// <summary>
        /// Is raised when a connection with a remote client has been established.
        /// </summary>
        public event EventHandler<ClientConnectionEventArgs> Connected;

        /// <summary>
        /// Is raised AFTER server has been started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Is raised when server begins start process.
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Is raised AFTER server has been stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Is raised when server begins stop process.
        /// </summary>
        public event EventHandler Stopping;

        #endregion Events (6)

        #region Methods (18)

        /// <summary>
        /// Starts listening for a TCP client connection.
        /// </summary>
        /// <param name="listener">The underlying listener.</param>
        /// <param name="throwException">
        /// Throw exception (<see langword="true" />) or not (<see langword="false" />).
        /// </param>
        /// <returns>
        /// Operation was successful (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="listener" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// The raised exception.
        /// </exception>
        protected bool? BeginAcceptingTcpClient(TcpListener listener, bool throwException = false)
        {
            if (listener == null)
            {
                return null;
            }

            try
            {
                listener.BeginAcceptTcpClient(this.EndAcceptingTcpClient, listener);
                return true;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, throwException);
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ConnectionWithClient_Disposed(object sender, EventArgs e)
        {
            var clientConn = (ConnectionWithClient)sender;

            try
            {
                clientConn.Disposed -= this.ConnectionWithClient_Disposed;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);
            }
            finally
            {
                this.InvokeForConnections((list, state) => list.Remove(state.Connection),
                                          new
                                          {
                                              Connection = clientConn,
                                          });
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ConnectionWithClient_Closed(object sender, EventArgs e)
        {
            var clientConn = (ConnectionWithClient)sender;

            try
            {
                clientConn.Closed -= this.ConnectionWithClient_Closed;

                clientConn.Host.RaiseEventHandler(this.Closed,
                                                  new ClientConnectionEventArgs(clientConn));
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);
            }
            finally
            {
                this.InvokeForConnections((list, state) => list.Remove(state.Connection),
                                          new
                                          {
                                              Connection = clientConn,
                                          });
            }
        }

        /// <summary>
        /// Creates the inner connection list.
        /// </summary>
        /// <returns>The new list.</returns>
        protected virtual IList<ConnectionWithClient> CreateConnectionList()
        {
            return null;
        }

        /// <summary>
        /// Disposes the old TCP listener.
        /// </summary>
        protected void DisposeOldListener()
        {
            try
            {
                var oldListener = this._listener;

                if (oldListener != null)
                {
                    using (oldListener.Server)
                    {
                        oldListener.Stop();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            this._listener = null;
        }

        /// <summary>
        /// The async callback for <see cref="ServerHost.BeginAcceptingTcpClient(TcpListener, bool)" /> method.
        /// </summary>
        /// <param name="ar">The async result.</param>
        protected void EndAcceptingTcpClient(IAsyncResult ar)
        {
            var waitForNext = true;

            ConnectionWithClient clientConn = null;
            TcpListener listener = null;
            RemoteConnection remoteConn = null;

            Action disposeConnections = () =>
                {
                    if (clientConn != null)
                    {
                        try
                        {
                            clientConn.Dispose();
                        }
                        catch (ObjectDisposedException)
                        {
                            // ignore
                        }
                        catch (Exception ex2)
                        {
                            this.RaiseError(ex2);
                        }
                    }
                    else if (remoteConn != null)
                    {
                        try
                        {
                            remoteConn.Dispose();
                        }
                        catch (ObjectDisposedException)
                        {
                            // ignore
                        }
                        catch (Exception ex2)
                        {
                            this.RaiseError(ex2);
                        }
                    }
                };

            try
            {
                listener = ar.AsyncState as TcpListener;
                if (listener == null)
                {
                    return;
                }

                var client = listener.EndAcceptTcpClient(ar);

                remoteConn = new RemoteConnection(this.Application, client.Client, true);

                clientConn = new ConnectionWithClient(this, remoteConn);
                clientConn.Closed += this.ConnectionWithClient_Closed;
                clientConn.Disposed += this.ConnectionWithClient_Disposed;

                this.InvokeForConnections((list, state) => list.Add(state.NewConnection),
                                          new
                                          {
                                              NewConnection = clientConn,
                                          });

                if (this.StartCommunicationWithClient(clientConn).IsFalse())
                {
                    disposeConnections();
                }
            }
            catch (ObjectDisposedException)
            {
                waitForNext = false;
            }
            catch (Exception ex)
            {
                disposeConnections();

                this.RaiseError(ex);
            }
            finally
            {
                if (waitForNext)
                {
                    this.BeginAcceptingTcpClient(listener);
                }
            }
        }

        /// <summary>
        /// Invokes an action for the internal list of connections thread safe.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeForConnections(Action<IList<ConnectionWithClient>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.InvokeForConnections((list, state) => state.Action(list),
                                      new
                                      {
                                          Action = action,
                                      });
        }

        /// <summary>
        /// Invokes an action for the internal list of connections thread safe.
        /// </summary>
        /// <typeparam name="TState">Type of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">
        /// The 2nd argument for <paramref name="action" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeForConnections<TState>(Action<IList<ConnectionWithClient>, TState> action, TState actionState)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.InvokeForConnections(action: action,
                                      actionStateFactory: (list) => actionState);
        }

        /// <summary>
        /// Invokes an action for the internal list of connections thread safe.
        /// </summary>
        /// <typeparam name="TState">Type of the 2nd argument for <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateFactory">
        /// The factory that produces the 2nd argument for <paramref name="action" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeForConnections<TState>(Action<IList<ConnectionWithClient>, TState> action, Func<IList<ConnectionWithClient>, TState> actionStateFactory)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            this.InvokeForConnections(func: (list, state) =>
                {
                    state.Action(list,
                                 state.StateFactory(list));

                    return (object)null;
                }, funcState: new
                {
                    Action = action,
                    StateFactory = actionStateFactory,
                });
        }

        /// <summary>
        /// Invokes a function for the internal list of connections thread safe.
        /// </summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeForConnections<TResult>(Func<IList<ConnectionWithClient>, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return this.InvokeForConnections((list, state) => state.Func(list),
                                             new
                                             {
                                                 Func = func,
                                             });
        }

        /// <summary>
        /// Invokes a function for the internal list of connections thread safe.
        /// </summary>
        /// <typeparam name="TState">Type of the 2nd argument for <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcState">
        /// The 2nd argument for <paramref name="func" />.
        /// </param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeForConnections<TState, TResult>(Func<IList<ConnectionWithClient>, TState, TResult> func, TState funcState)
        {
            return this.InvokeForConnections<TState, TResult>(func: func,
                                                              funcStateFactory: (list) => funcState);
        }

        /// <summary>
        /// Invokes a function for the internal list of connections thread safe.
        /// </summary>
        /// <typeparam name="TState">Type of the 2nd argument for <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcStateFactory">
        /// The factory that produces the 2nd argument for <paramref name="func" />.
        /// </param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeForConnections<TState, TResult>(Func<IList<ConnectionWithClient>, TState, TResult> func, Func<IList<ConnectionWithClient>, TState> funcStateFactory)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (funcStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            TResult result;

            lock (this._SYNC)
            {
                result = func(this._CONNECTIONS,
                              funcStateFactory(this._CONNECTIONS));
            }

            return result;
        }

        /// <summary>
        /// <see cref="DisposableBase.OnDispose(bool, ref bool)" />
        /// </summary>
        protected override void OnDispose(bool disposing, ref bool isDisposed)
        {
            try
            {
                this.OnStop(disposing, ref isDisposed);
            }
            catch (Exception ex)
            {
                if (disposing)
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// The logic for the <see cref="ServerHost.Start()" /> method.
        /// </summary>
        /// <param name="isRunning">
        /// The new value for the <see cref="ServerHost.IsRunning" /> property.
        /// </param>
        protected virtual void OnStart(ref bool isRunning)
        {
            this.DisposeOldListener();

            var newListener = new TcpListener(this.Settings.Address ?? IPAddress.Any,
                                              this.Settings.Port);

            newListener.Start();

            this.BeginAcceptingTcpClient(newListener, true);

            this._listener = newListener;
        }

        /// <summary>
        /// The logic for the <see cref="ServerHost.Stop()" /> method.
        /// </summary>
        /// <param name="disposing">
        /// <see cref="DisposableBase.Dispose()" /> method was invoked (<see langword="true" />)
        /// or the destructor (<see langword="false" />).
        /// <see langword="null" /> indicates that <see cref="ServerHost.Stop()" /> method was invoked.
        /// </param>
        /// <param name="isRunning">
        /// The new value for the <see cref="ServerHost.IsRunning" /> property.
        /// </param>
        protected virtual void OnStop(bool? disposing, ref bool isRunning)
        {
            try
            {
                this.DisposeOldListener();

                // close open connections
                this.InvokeForConnections((list, state) =>
                    {
                        foreach (var conn in list)
                        {
                            try
                            {
                                conn.Dispose();
                            }
                            catch (ObjectDisposedException)
                            {
                                // ignore
                            }
                            catch (Exception ex)
                            {
                                this.RaiseError(ex);
                            }
                        }
                    }, new
                    {
                        Disposing = disposing,
                    });
            }
            catch (Exception ex)
            {
                if (disposing.IsTrue())
                {
                    this.RaiseError(ex, true);
                }
            }
        }

        /// <summary>
        /// Starts the host.
        /// </summary>
        /// <exception cref="ApplicationException">An error occurred.</exception>
        public void Start()
        {
            try
            {
                lock (this._SYNC)
                {
                    this.ThrowIfDisposed();

                    if (this.IsRunning)
                    {
                        return;
                    }

                    this.RaiseEventHandler(this.Starting);

                    var isRunning = true;
                    this.OnStart(ref isRunning);

                    this.IsRunning = isRunning;
                    if (this.IsRunning)
                    {
                        this.RaiseEventHandler(this.Started);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, true);
            }
        }

        /// <summary>
        /// Starts a new connection with a client.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        /// Operation was successful or not.
        /// <see langword="null" /> indicates that <paramref name="connection" /> is <see langword="null" />.
        /// </returns>
        protected virtual bool? StartCommunicationWithClient(ConnectionWithClient connection)
        {
            if (connection == null)
            {
                return null;
            }

            try
            {
                Task.Factory
                    .StartNew((state) =>
                              {
                                  var c = (ConnectionWithClient)state;

                                  try
                                  {
                                      using (c)
                                      {
                                          c.Host.RaiseEventHandler(this.Connected,
                                                                   new ClientConnectionEventArgs(c));

                                          c.Start();
                                      }
                                  }
                                  catch (ObjectDisposedException)
                                  {
                                      // ignore
                                  }
                                  catch (Exception ex)
                                  {
                                      c.Host.RaiseError(ex);
                                  }
                              }, state: connection);

                return true;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);
                return false;
            }
        }

        /// <summary>
        /// Stops the host.
        /// </summary>
        /// <exception cref="ApplicationException">An error occurred.</exception>
        public void Stop()
        {
            try
            {
                lock (this._SYNC)
                {
                    this.ThrowIfDisposed();

                    if (!this.IsRunning)
                    {
                        return;
                    }

                    this.RaiseEventHandler(this.Stopping);

                    var isRunning = false;
                    this.OnStop(null, ref isRunning);

                    this.IsRunning = isRunning;
                    if (!this.IsRunning)
                    {
                        this.RaiseEventHandler(this.Stopped);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RaiseError(ex, true);
            }
        }

        #endregion Methods (18)
    }
}