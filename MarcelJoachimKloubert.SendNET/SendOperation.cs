// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Helpers;
using System;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// Proccesses an operation.
    /// </summary>
    public sealed class SendOperation
    {
        #region Fields (1)

        private readonly AppSettings _SETTINGS;

        #endregion Fields (1)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="SendOperation" /> class.
        /// </summary>
        /// <param name="settings">The underlying applications ettings.</param>
        public SendOperation(AppSettings settings)
        {
            this._SETTINGS = settings;
        }

        #endregion Constructors (1)

        #region Properties (1)

        /// <summary>
        /// Gets the underlying application settings.
        /// </summary>
        public AppSettings Settings
        {
            get { return this._SETTINGS; }
        }

        #endregion Properties (1)

        #region Methods (6)

        private Binding CreateBinding()
        {
            var result = new NetTcpBinding();
            result.CloseTimeout = TimeSpan.FromSeconds(10.0f);
            result.MaxBufferPoolSize = int.MaxValue;
            result.MaxReceivedMessageSize = int.MaxValue;
            result.OpenTimeout = result.CloseTimeout;
            result.ReaderQuotas.MaxArrayLength = int.MaxValue;
            result.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            result.ReaderQuotas.MaxDepth = int.MaxValue;
            result.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
            result.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            result.ReceiveTimeout = TimeSpan.FromDays(1);
            result.ReliableSession.Enabled = true;
            result.ReliableSession.InactivityTimeout = result.ReceiveTimeout;
            result.Security.Mode = SecurityMode.None;
            result.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            result.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            result.SendTimeout = result.ReceiveTimeout;
            result.TransactionFlow = false;
            result.TransferMode = TransferMode.Buffered;

            return result;
        }

        private void CreateConnectionData(out Binding binding, out EndpointAddress ep)
        {
            binding = this.CreateBinding();
            ep = this.CreateEndpoint();
        }

        private EndpointAddress CreateEndpoint()
        {
            var hostAddr = Environment.MachineName;
            var port = this.Settings.Port;

            var uri = new Uri(string.Format("net.tcp://{0}:{1}/MarcelJoachimKloubert/SendNET",
                                            hostAddr, port));

            return new EndpointAddress(uri);
        }

        private void ReceiveFiles()
        {
            this.Settings.TargetDirectory.Refresh();
            if (this.Settings.TargetDirectory.Exists == false)
            {
                this.Settings.TargetDirectory.Create();
                this.Settings.TargetDirectory.Refresh();
            }

            using (var host = new ServiceHost(typeof(global::MarcelJoachimKloubert.SendNET.Contracts.SendDataService)))
            {
                Binding binding;
                EndpointAddress ep;
                this.CreateConnectionData(out binding, out ep);

                host.AddServiceEndpoint(typeof(global::MarcelJoachimKloubert.SendNET.Contracts.ISendDataService),
                                        binding,
                                        ep.Uri);
                
                Console.WriteLine();

                ConsoleHelper.InvokeForColor(() => Console.Write("Start lisenting on port '{0}'... ",
                                                                 ep.Uri.Port),
                                             ConsoleColor.White);

                try
                {
                    host.Open();

                    ConsoleHelper.InvokeForColor(() => Console.WriteLine("[OK]"),
                                                 ConsoleColor.Green);

                    Console.WriteLine();

                    Console.WriteLine("===== Press enter to shutdown =====");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    var innerEx = ex.GetBaseException() ?? ex;

                    ConsoleHelper.InvokeForColor(() => Console.WriteLine("[ERROR: '{0}' {1}]",
                                                                         innerEx.GetType().FullName,
                                                                         innerEx.Message),
                                                 ConsoleColor.Red);
                }
            }
        }

        private void SendFiles()
        {
            using (var host = new ServiceHost(typeof(global::MarcelJoachimKloubert.SendNET.Contracts.SendDataService)))
            {
                Binding binding;
                EndpointAddress ep;
                this.CreateConnectionData(out binding, out ep);

                var client = new MarcelJoachimKloubert.SendNET.Client.SendDataClient(this.Settings,
                                                                                     binding,
                                                                                     ep);
                using (var conn = client.Connect())
                {
                    foreach (var file in this.Settings.FilesToSend)
                    {
                        try
                        {
                            ConsoleHelper.InvokeForColor(() => Console.Write("Sending file '{0}'... ", file.FullName),
                                                            ConsoleColor.White);
                            conn.SendFile(file.FullName);

                            ConsoleHelper.InvokeForColor(() => Console.WriteLine("[OK]"),
                                                            ConsoleColor.Green);
                        }
                        catch (Exception ex)
                        {
                            var innerEx = ex.GetBaseException() ?? ex;

                            ConsoleHelper.InvokeForColor(() => Console.WriteLine("[ERROR: '{0}' {1}]",
                                                                                    innerEx.GetType().FullName,
                                                                                    innerEx.Message),
                                                            ConsoleColor.Red);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts the operations.
        /// </summary>
        public void Start()
        {
            Action actionToInvoke = null;

            switch (this.Settings.Type)
            {
                case SendOperationType.ReceiveFiles:
                    actionToInvoke = this.ReceiveFiles;
                    break;

                case SendOperationType.SendFiles:
                    actionToInvoke = this.SendFiles;
                    break;
            }

            if (actionToInvoke != null)
            {
                actionToInvoke();
            }
        }

        #endregion Methods (3)
    }
}