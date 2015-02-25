// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Cryptography;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace MarcelJoachimKloubert.SendNET.Client
{
    partial class SendDataClient
    {
        private sealed class SendDataClientConnection : ISendDataClientConnection
        {
            #region Constructors (2)

            internal SendDataClientConnection(SendDataClient client)
            {
                this.Client = client;
            }

            ~SendDataClientConnection()
            {
                this.Dispose(false);
            }

            #endregion Constructors (2)

            #region Properties (3)

            internal SendDataClient Client
            {
                get;
                private set;
            }

            public ICrypter Crypter
            {
                get;
                internal set;
            }

            public Guid Id
            {
                get;
                internal set;
            }

            #endregion Properties (3)

            #region Methods (6)

            public void Disconnection()
            {
                this.Client
                    .Disconnect();
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Disconnection();
                }
            }

            private byte[] EnryptXml(XDocument xml)
            {
                using (var uncryptedData = new MemoryStream())
                {
                    xml.Save(uncryptedData);

                    uncryptedData.Position = 0;
                    using (var cryptedData = new MemoryStream())
                    {
                        this.Crypter
                            .Encrypt(uncryptedData, cryptedData);

                        return cryptedData.ToArray();
                    }
                }
            }

            public void SendFile(string localPath)
            {
                var localFile = new FileInfo(localPath);

                var tmpFile = new FileInfo(Path.GetTempFileName());
                try
                {
                    using (var localFileStream = localFile.OpenRead())
                    {
                        using (var cryptedFileStream = tmpFile.Open(FileMode.Open, FileAccess.ReadWrite))
                        {
                            this.Crypter
                                .Encrypt(localFileStream, cryptedFileStream);

                            cryptedFileStream.Position = 0;
                            this.SendFile(cryptedFileStream, localFile.Name);
                        }
                    }
                }
                finally
                {
                    // delete temp file
                    try
                    {
                        tmpFile.Refresh();
                        if (tmpFile.Exists)
                        {
                            tmpFile.Delete();
                        }
                    }
                    catch
                    {
                        // ignore errors here
                    }
                }
            }

            private void SendFile(Stream stream, string filename)
            {
                // XML data for setting up file
                var xmlMeta = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                {
                    xmlMeta.Add(new XElement("file"));

                    xmlMeta.Root
                           .Add(new XElement("name", filename.Trim()));
                }

                // setup target file
                this.Client
                    .Channel.SetupFile(this.EnryptXml(xmlMeta));

                // send data
                this.Client
                    .Channel.SendFile(stream);
            }

            #endregion Methods (6)
        }
    }
}