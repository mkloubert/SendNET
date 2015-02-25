// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Contracts;
using MarcelJoachimKloubert.SendNET.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml.Linq;

namespace MarcelJoachimKloubert.SendNET.Client
{
    /// <summary>
    /// Client for sending data.
    /// </summary>
    public sealed partial class SendDataClient : ClientBase<ISendDataService>
    {
        #region Fields (2)

        private ICrypter _crypter;
        private readonly AppSettings _SETTINGS;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDataClient" /> class.
        /// </summary>
        /// <param name="settings">The app settings.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddr">The remote address of the host.</param>
        public SendDataClient(AppSettings settings, Binding binding, EndpointAddress remoteAddr)
            : base(binding, remoteAddr)
        {
            this._SETTINGS = settings;
        }

        #endregion Constructors (1)

        #region Properties (2)

        /// <summary>
        /// Gets the underlying crypter.
        /// </summary>
        public ICrypter Crypter
        {
            get { return this._crypter; }
        }

        /// <summary>
        /// Gets the underlying settings.
        /// </summary>
        public AppSettings Settings
        {
            get { return this._SETTINGS; }
        }

        #endregion Properties (2)

        #region Methods (2)

        /// <inheriteddoc />
        public ISendDataClientConnection Connect()
        {
            byte[] uncryptedResult;
            using (var rsa = new RSACryptoServiceProvider())
            {
                var meta = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                meta.Add(new XElement("sendnet"));

                var pubkeyElement = new XElement("publicKey");
                pubkeyElement.Value = Convert.ToBase64String(rsa.ExportCspBlob(false));

                meta.Root.Add(pubkeyElement);

                using (var metaStream = new MemoryStream())
                {
                    meta.Save(metaStream);

                    uncryptedResult = rsa.Decrypt(this.Channel.Connect(metaStream.ToArray()),
                                                  false);
                }
            }

            try
            {
                var result = new SendDataClientConnection(this);
                result.Id = new Guid(uncryptedResult.Take(16).ToArray());
                result.Crypter = new RijndaelCrypter(uncryptedResult.Skip(16).ToArray(),
                                                     this.Settings.Salt,
                                                     this.Settings.Iterations);

                this._crypter = result.Crypter;
                return result;
            }
            finally
            {
                uncryptedResult = new byte[uncryptedResult.Length];
            }
        }

        /// <inheriteddoc />
        public void Disconnect()
        {
            this.Channel
                .Disconnect();
        }

        #endregion Methods (2)
    }
}