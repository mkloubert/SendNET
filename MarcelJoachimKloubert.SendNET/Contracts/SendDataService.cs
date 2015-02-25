// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Cryptography;
using MarcelJoachimKloubert.SendNET.Helpers;
using System;
using System.IO;
using System.IO.Compression;
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
    [ServiceBehavior(AutomaticSessionShutdown = false,
                     ConcurrencyMode = ConcurrencyMode.Single,
                     IncludeExceptionDetailInFaults = true,
                     InstanceContextMode = InstanceContextMode.PerSession)]
    public sealed class SendDataService : ISendDataService
    {
        #region Fields (6)

        private ICrypter _crypter;
        private bool? _faulted = null;
        private FileStream _targetStream;

        /// <summary>
        /// Name of the attribute that defines if data is compressed or not.
        /// </summary>
        public const string XML_ATTRIB_NAME_IS_COMPRESSED = "isCompressed";

        /// <summary>
        /// XML value for <see langword="false" />.
        /// </summary>
        public const string XML_VALUE_FALSE = "0";

        /// <summary>
        /// XML value for <see langword="true" />.
        /// </summary>
        public const string XML_VALUE_TRUE = "1";

        #endregion Fields (6)

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

        #region Methods (9)

        /// <inheriteddoc />
        public byte[] CloseFile()
        {
            try
            {
                using (var targetStream = this._targetStream)
                {
                    this._targetStream = null;
                }

                ConsoleHelper.InvokeForColor(() => Console.WriteLine("[OK]"),
                                                                     ConsoleColor.Green);
            }
            catch
            {
                this._faulted = true;

                throw;
            }

            return null;
        }

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
        public byte[] Disconnect()
        {
            this.DispatchService();

            return null;
        }

        private void DispatchService()
        {
            // remove CLOSED EVENT
            try
            {
                OperationContext.Current.InstanceContext.Closed -= this.SendDataService_InstanceContext_Closed;
            }
            catch
            {
                // ignore here
            }

            // remove FAULTED EVENT
            try
            {
                OperationContext.Current.InstanceContext.Faulted -= this.SendDataService_InstanceContext_Faulted;
            }
            catch
            {
                // ignore here
            }

            // dispose TARGET STREAM
            try
            {
                using (var targetStream = this._targetStream)
                {
                    this._targetStream = null;
                }
            }
            catch
            {
                // ignore here
            }

            // try delete file, because it is faulted
            if (this._faulted == true)
            {
                var targetStream = this._targetStream;
                if (targetStream != null)
                {
                    try
                    {
                        var targetFile = new FileInfo(targetStream.Name);
                        if (targetFile.Exists)
                        {
                            targetFile.Delete();
                        }
                    }
                    catch
                    {
                        // ignore here
                    }
                }
            }

            // dispose CRYPTER
            try
            {
                using (var c = this._crypter)
                {
                    this._crypter = null;
                }
            }
            catch
            {
                // ignore here
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
        public byte[] OpenFile(byte[] meta)
        {
            if (this._targetStream != null)
            {
                throw new Exception("File already open!");
            }

            FileInfo targetFile = null;

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
                targetFile = new FileInfo(Path.Combine(Settings.TargetDirectory.FullName,
                                                       fullname));
                while (targetFile.Exists)
                {
                    targetFile = new FileInfo(Path.Combine(Settings.TargetDirectory.FullName,
                                                           string.Format("{0}-{1}{2}",
                                                                         baseName, index++, ext)));
                }
            }

            try
            {
                this._faulted = false;

                this._targetStream = targetFile.Open(FileMode.CreateNew, FileAccess.ReadWrite);
            }
            catch
            {
                this._faulted = true;

                targetFile.Refresh();
                if (targetFile.Exists)
                {
                    targetFile.Delete();
                }

                throw;
            }

            Console.WriteLine();
            Console.WriteLine();

            ConsoleHelper.InvokeForColor(() => Console.Write("Receiving file '{0}'... ", targetFile.Name),
                                         ConsoleColor.White);

            return null;
        }

        private static void OutputException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            var innerEx = ex.GetBaseException() ?? ex;

            ConsoleHelper.InvokeForColor(() => Console.WriteLine("[ERROR: '{0}' {1}]",
                                                                 innerEx.GetType().FullName,
                                                                 innerEx.Message),
                                               ConsoleColor.Red);
        }

        /// <inheriteddoc />
        public byte[] WriteFile(byte[] meta)
        {
            var targetStream = this._targetStream;
            if (targetStream == null)
            {
                throw new Exception("No file!");
            }

            if (meta == null ||
                meta.Length == 0)
            {
                throw new Exception("No meta defined!");
            }

            var xml = this.DecryptXml(meta);

            var data = Convert.FromBase64String(xml.Root
                                                   .Value.Trim());
            try
            {
                var isCompressed = XML_VALUE_TRUE == xml.Root.Attribute(XML_ATTRIB_NAME_IS_COMPRESSED).Value;
                if (isCompressed)
                {
                    using (var compressedStream = new MemoryStream(data))
                    {
                        using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress, true))
                        {
                            using (var uncompressedStream = new MemoryStream())
                            {
                                gzip.CopyTo(uncompressedStream);

                                data = uncompressedStream.ToArray();
                            }
                        }
                    }
                }

                targetStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                this._faulted = true;

                OutputException(ex);

                throw;
            }

            return null;
        }

        #endregion Methods (9)
    }
}