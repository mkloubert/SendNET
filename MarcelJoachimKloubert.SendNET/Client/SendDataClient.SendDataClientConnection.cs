// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Contracts;
using MarcelJoachimKloubert.SendNET.Cryptography;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

            #region Methods (7)

            private static XElement CreateSendFileDataElement(byte[] uncompressedData)
            {
                var result = new XElement("data");

                using (var compressedStream = new MemoryStream())
                {
                    using (var gzip = new GZipStream(compressedStream, CompressionMode.Compress, true))
                    {
                        gzip.Write(uncompressedData, 0, uncompressedData.Length);
                    }

                    var isCompressed = compressedStream.Length < uncompressedData.Length;

                    // is compressed attribute
                    result.SetAttributeValue(SendDataService.XML_ATTRIB_NAME_IS_COMPRESSED,
                                             isCompressed ? SendDataService.XML_VALUE_TRUE
                                                          : SendDataService.XML_VALUE_FALSE);

                    // set data
                    result.Value = Convert.ToBase64String(isCompressed ? compressedStream.ToArray()
                                                                       : uncompressedData);
                }

                return result;
            }

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
                        this.SendFile(localFileStream,
                                      localFile.Name);
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

                this.Client.Channel
                           .OpenFile(this.EnryptXml(xmlMeta));

                try
                {
                    var buffer = new byte[this.Client.Settings
                                                     .BufferSize];

                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var xml = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                        xml.Add(CreateSendFileDataElement(buffer.Take(bytesRead)
                                                                .ToArray()));

                        this.Client
                            .Channel.WriteFile(this.EnryptXml(xml));
                    }
                }
                finally
                {
                    this.Client.Channel
                               .CloseFile();
                }
            }

            #endregion Methods (7)
        }
    }
}