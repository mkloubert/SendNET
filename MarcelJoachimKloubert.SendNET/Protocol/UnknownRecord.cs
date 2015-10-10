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

using MarcelJoachimKloubert.SendNET.ComponentModel;
using MarcelJoachimKloubert.SendNET.Cryptography;
using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace MarcelJoachimKloubert.SendNET.Protocol
{
    /// <summary>
    /// A generic / unknown record.
    /// </summary>
    [Record(RecordType.UNKNOWN)]
    public class UnknownRecord : NotifiableBase, IRecord
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownRecord" /> class.
        /// </summary>
        /// <param name="crypter">The value for the <see cref="UnknownRecord.Crypter" /> property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="crypter" /> is <see langword="null" />.
        /// </exception>
        public UnknownRecord(ICrypter crypter)
            : base()
        {
            if (crypter == null)
            {
                throw new ArgumentNullException("crypter");
            }

            this.Crypter = crypter;
        }

        #endregion Constructors (1)

        #region Properties (6)

        /// <summary>
        /// Gets or sets the compresseion for <see cref="UnknownRecord.Content" />.
        /// </summary>
        public ContentCompression Compression
        {
            get { return this.Get(() => this.Compression); }

            set { this.Set(() => this.Compression, value); }
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The new data is too big.</exception>
        public virtual byte[] Content
        {
            get { return this.Get(() => this.Content); }

            set
            {
                if (value != null)
                {
                    if (value.Length > ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value.Length", value,
                                                              string.Format("Cannot be larger than {0} bytes!",
                                                                            ushort.MaxValue));
                    }
                }

                this.Set(() => this.Content, value);
            }
        }

        /// <summary>
        /// Gets the underyling crypter.
        /// </summary>
        public ICrypter Crypter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        public uint RequestId
        {
            get { return this.Get(() => this.RequestId); }

            set { this.Set(() => this.RequestId, value); }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public virtual ushort Type
        {
            get { return this.Get(() => this.Type); }

            set { this.Set(() => this.Type, value); }
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public byte Version
        {
            get { return this.Get(() => this.Version); }

            set { this.Set(() => this.Version, value); }
        }

        #endregion Properties (6)

        #region Methods (5)

        /// <summary>
        /// Creates instances for a stream.
        /// </summary>
        /// <param name="stream">The stream from where to read the data from.</param>
        /// <param name="crypter">The optional crypter to use.</param>
        /// <returns>The instances.</returns>
        /// <exception cref="ArgumentNullException">
        /// At least one argument is <see langword="null" />.
        /// </exception>
        public static IEnumerable<UnknownRecord> FromStream(NetworkStream stream, ICrypter crypter)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte? @byte;
            byte[] buffer;

            if ((@byte = stream.WaitAndReadByte()) == null)
            {
                yield break;
            }

            // version
            var version = @byte.Value;

            buffer = stream.WaitAndRead(2);
            if (buffer.Length != 2)
            {
                yield break;
            }

            // type
            var type = buffer.ToUInt16().Value;

            buffer = stream.WaitAndRead(4);
            if (buffer.Length != 4)
            {
                yield break;
            }

            // request ID
            var requestId = buffer.ToUInt32().Value;

            buffer = stream.WaitAndRead(4);
            if (buffer.Length != 4)
            {
                yield break;
            }

            // content length
            var contentLength = buffer.ToUInt32().Value;
            if (contentLength > 1048575)
            {
                contentLength = 1048575;
            }

            if ((@byte = stream.WaitAndReadByte()) == null)
            {
                yield break;
            }

            // compression
            ContentCompression compression;
            Enum.TryParse<ContentCompression>(@byte.ToString(), out compression);

            buffer = stream.WaitAndRead((int)contentLength);
            if (buffer.Length != contentLength)
            {
                yield break;
            }

            var content = buffer;

            RecordType knownType;
            Enum.TryParse<RecordType>(type.ToString(), out knownType);

            var recordType = Assembly.GetExecutingAssembly()
                                     .GetTypes()
                                     .Where(x =>
                                            {
                                                var allAttribs = x.GetCustomAttributes(typeof(RecordAttribute), false)
                                                                  .Cast<RecordAttribute>()
                                                                  .ToArray();

                                                if (allAttribs.Length < 1)
                                                {
                                                    return false;
                                                }

                                                return allAttribs.First().Type == knownType;
                                            })
                                     .FirstOrDefault();

            if (recordType == null)
            {
                yield break;
            }

            var result = (UnknownRecord)Activator.CreateInstance(type: recordType,
                                                                 args: new object[] { crypter });

            result.Compression = compression;
            result.Crypter = crypter;
            result.RequestId = requestId;
            result.Version = version;

            if (!result.ParseContent(content))
            {
                yield break;
            }

            yield return result;
        }

        /// <summary>
        /// The logic for the <see cref="UnknownRecord.ParseContent(IEnumerable{byte})" /> method.
        /// </summary>
        /// <param name="newContent">The new content.</param>
        /// <param name="success">
        /// The result for <see cref="UnknownRecord.ParseContent(IEnumerable{byte})" />.
        /// Is <see langword="false" /> by default.
        /// </param>
        protected virtual void OnParseContent(byte[] newContent, ref bool success)
        {
            this.Content = newContent;
            success = true;
        }

        /// <summary>
        /// Parses binary data for the properties (especially <see cref="UnknownRecord.Content" />).
        /// </summary>
        /// <param name="newContent">The new content.</param>
        /// <returns>Operation was successful / data was applied or not.</returns>
        public bool ParseContent(IEnumerable<byte> newContent)
        {
            var result = false;

            try
            {
                var content = this.Crypter.Decrypt(newContent.AsArray());

                switch (this.Compression)
                {
                    case ContentCompression.None:
                        break;

                    case ContentCompression.GZip:
                        using (var uncompressedStream = new MemoryStream())
                        {
                            using (var compressedStream = new MemoryStream(content, false))
                            {
                                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress, true))
                                {
                                    gzip.CopyTo(uncompressedStream);
                                }
                            }

                            content = uncompressedStream.ToArray();
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                this.OnParseContent(content,
                                    ref result);
            }
            catch (NotSupportedException nsex)
            {
                this.RaiseError(nsex);

                throw nsex;
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);

                result = false;
            }

            return result;
        }

        /// <summary>
        /// Sends that record over a remote connection.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="conn" /> is <see langword="null" />.
        /// </exception>
        public void SendTo(IRemoteConnection conn)
        {
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            var blob = this.ToByteArray();
            conn.Stream.Write(blob, 0, blob.Length);
        }

        /// <summary>
        /// Returns the data of that recors as byte array.
        /// </summary>
        /// <returns>The binary data of that record.</returns>
        public byte[] ToByteArray()
        {
            using (var temp = new MemoryStream())
            {
                var content = this.Content ?? new byte[0];

                var compression = ContentCompression.None;

                // version
                temp.WriteByte(this.Version);

                // type
                temp.Write(this.Type.GetBytes(), 0, 2);

                // request ID
                temp.Write(this.RequestId.GetBytes(), 0, 4);

                if (content.Length > 0)
                {
                    // test if it makes sense to compress the content

                    using (var compressedStream = new MemoryStream())
                    {
                        using (var contentStream = new MemoryStream(content, false))
                        {
                            using (var gzip = new GZipStream(compressedStream, CompressionMode.Compress, true))
                            {
                                contentStream.CopyTo(gzip);

                                gzip.Flush();
                                gzip.Close();
                            }
                        }

                        if (compressedStream.Length < content.Length)
                        {
                            // yes => compress

                            compression = ContentCompression.GZip;

                            content = compressedStream.ToArray();
                        }
                    }
                }

                // nor encrypt
                content = this.Crypter.Encrypt(content);
                var contentLength = (uint)content.Length;

                // content length
                temp.Write(contentLength.GetBytes(), 0, 4);

                // compression
                temp.WriteByte((byte)compression);

                // content
                temp.Write(content, 0, content.Length);

                return temp.ToArray();
            }
        }

        #endregion Methods (5)
    }
}