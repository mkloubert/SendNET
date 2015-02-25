// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Cryptography;
using MarcelJoachimKloubert.SendNET.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml.Linq;

namespace MarcelJoachimKloubert.SendNET.Contracts
{
    /// <summary>
    /// Implementation of <see cref="ISendDataService" /> interface.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
                     ConcurrencyMode = ConcurrencyMode.Multiple,
                     InstanceContextMode = InstanceContextMode.PerSession)]
    public sealed class SendDataService : ISendDataService
    {
        #region Fields (2)

        private ICrypter _crypter;
        private FileInfo _targetFile;

        #endregion Fields (2)

        #region Constructors (1)

        public SendDataService()
        {
            OperationContext.Current.InstanceContext.Closed += this.SendDataService_InstanceContext_Closed;
            OperationContext.Current.InstanceContext.Faulted += this.SendDataService_InstanceContext_Faulted;
        }

        ~SendDataService()
        {
            this.DispatchService();
        }

        #endregion Constructors (1)

        #region Events and delegates (2)

        private void SendDataService_InstanceContext_Closed(object sender, EventArgs e)
        {
            this.DispatchService();
        }

        private void SendDataService_InstanceContext_Faulted(object sender, EventArgs e)
        {
            this.DispatchService();
        }

        #endregion Events and delegates (2)

        #region Properties (2)

        /// <summary>
        /// Gets the underlying crypter.
        /// </summary>
        public ICrypter Crypter
        {
            get { return this._crypter; }
        }

        /// <summary>
        /// Gets or sets the global application settings.
        /// </summary>
        public static AppSettings Settings
        {
            get;
            set;
        }

        #endregion Properties (2)

        #region Methods (8)

        /// <inheriteddoc />
        public byte[] Connect(byte[] meta)
        {
            var rng = new RNGCryptoServiceProvider();
            var rand = new Random();

            XDocument xmlMeta;
            using (var temp = new MemoryStream(meta))
            {
                xmlMeta = XDocument.Load(temp);
            }

            var publicKey = Convert.FromBase64String(xmlMeta.Root.Element("publicKey").Value.Trim());
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.ImportCspBlob(publicKey);

                var id = Guid.NewGuid();

                var pwd = new byte[64];
                rng.GetBytes(pwd);

                var crypter = new RijndaelCrypter(pwd, Settings.Salt, Settings.Iterations);

                using (var uncryptedResult = new MemoryStream())
                {
                    byte[] buffer;

                    // connection ID
                    buffer = id.ToByteArray();
                    uncryptedResult.Write(buffer, 0, buffer.Length);

                    // password
                    uncryptedResult.Write(pwd, 0, pwd.Length);

                    this._crypter = crypter;
                    return rsa.Encrypt(uncryptedResult.ToArray(),
                                       false);
                }
            }
        }

        private byte[] CryptXml(XDocument xml)
        {
            var crypter = this._crypter;
            if (crypter == null)
            {
                return null;
            }

            using (var uncryptedXml = new MemoryStream())
            {
                xml.Save(uncryptedXml);

                uncryptedXml.Position = 0;
                using (var cryptedXml = new MemoryStream())
                {
                    crypter.Encrypt(uncryptedXml, cryptedXml);

                    return cryptedXml.ToArray();
                }
            }
        }

        private XDocument DecryptXml(byte[] crypted)
        {
            var crypter = this._crypter;
            if (crypter == null)
            {
                return null;
            }

            using (var cryptedXml = new MemoryStream(crypted))
            {
                using (var uncryptedXml = new MemoryStream())
                {
                    crypter.Decrypt(cryptedXml, uncryptedXml);

                    uncryptedXml.Position = 0;
                    return XDocument.Load(uncryptedXml);
                }
            }
        }

        /// <inheriteddoc />
        public void Disconnect()
        {
            this.DispatchService();
        }

        private void DispatchService()
        {
            try
            {
                OperationContext.Current.InstanceContext.Closed -= this.SendDataService_InstanceContext_Closed;
            }
            catch
            {
            }

            try
            {
                OperationContext.Current.InstanceContext.Faulted -= this.SendDataService_InstanceContext_Faulted;
            }
            catch
            {
            }

            using (var c = this._crypter)
            {
                this._crypter = null;
            }
        }

        private static RemoteEndpointMessageProperty GetRemoteAddress()
        {
            var context = OperationContext.Current;
            if (context == null)
            {
                return null;
            }

            var prop = context.IncomingMessageProperties;
            if (prop == null)
            {
                return null;
            }

            return prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
        }

        /// <inheriteddoc />
        public void SendFile(Stream src)
        {
            var targetFile = this._targetFile;
            if (targetFile == null)
            {
                throw new FaultException("No file name defined!");
            }

            try
            {
                var remoteAddr = GetRemoteAddress();

                Console.WriteLine();
                ConsoleHelper.InvokeForColor(() => Console.Write("Receiving file '{0}'{1} ... ",
                                                                 targetFile.Name,
                                                                 remoteAddr != null ? string.Format(" from {0}:{1}",
                                                                                                    remoteAddr.Address, remoteAddr.Port)
                                                                                    : string.Empty),
                                             ConsoleColor.White);

                var tmpFile = new FileInfo(Path.GetTempFileName());
                try
                {
                    using (var tempStream = tmpFile.Open(FileMode.Open, FileAccess.ReadWrite))
                    {
                        src.CopyTo(tempStream);

                        tempStream.Position = 0;
                        using (var targetStream = targetFile.Open(FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            this.Crypter
                                .Decrypt(tempStream, targetStream);
                        }
                    }
                }
                finally
                {
                    tmpFile.Delete();
                }

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

                throw;
            }
            finally
            {
                this._targetFile = null;
            }
        }

        /// <inheriteddoc />
        public void SetupFile(byte[] meta)
        {
            var xml = this.DecryptXml(meta);

            var filename = new StringBuilder(xml.Root.Element("name").Value.Trim());
            for (var i = 0; i < filename.Length; i++)
            {
                if (Path.GetInvalidFileNameChars().Contains(filename[i]))
                {
                    filename[i] = '_';
                }
            }

            var fullname = filename.ToString().Trim();
            if (fullname != string.Empty)
            {
                var baseName = Path.GetFileNameWithoutExtension(fullname);
                var ext = Path.GetExtension(fullname);

                ulong index = 0;
                var targetFile = new FileInfo(Path.Combine(Settings.TargetDirectory.FullName,
                                                           fullname));
                while (targetFile.Exists)
                {
                    targetFile = new FileInfo(Path.Combine(Settings.TargetDirectory.FullName,
                                                           string.Format("{0}-{1}{2}",
                                                                         baseName, index++, ext)));
                }

                this._targetFile = targetFile;
            }
        }

        #endregion Methods (6)
    }
}