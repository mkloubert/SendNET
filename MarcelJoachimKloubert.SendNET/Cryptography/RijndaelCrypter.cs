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
using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace MarcelJoachimKloubert.SendNET.Cryptography
{
    /// <summary>
    /// A Rijndael crypter.
    /// </summary>
    public class RijndaelCrypter : NotifiableBase, ICrypter
    {
        #region Constructors (1)

        /// <summary>
        /// Creates a new instance of the <see cref="RijndaelCrypter" /> class.
        /// </summary>
        public RijndaelCrypter()
        {
            var key = new byte[48];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            this.Iterations = 1000;
            this.Key = key;
            this.Salt = Guid.NewGuid().ToByteArray();
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// Gets or sets the iterations.
        /// </summary>
        public uint Iterations
        {
            get { return this.Get(() => this.Iterations); }

            set
            {
                if ((value < 0) || (value > int.MaxValue))
                {
                    throw new ArgumentOutOfRangeException("value", value,
                                                          string.Format("Must be between {0} and {1}!",
                                                                        uint.MinValue, int.MaxValue));
                }

                this.Set(() => this.Iterations, value);
            }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public byte[] Key
        {
            get { return this.Get(() => this.Key); }

            set
            {
                if (value != null)
                {
                    if (value.Length > ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value.Length", value.Length,
                                                              string.Format("Cannot be larger than {0} bytes!", ushort.MaxValue));
                    }
                }

                this.Set(() => this.Key, value);
            }
        }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        public byte[] Salt
        {
            get { return this.Get(() => this.Salt); }

            set
            {
                if (value != null)
                {
                    if (value.Length > ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value.Length", value.Length,
                                                              string.Format("Cannot be larger than {0} bytes!", ushort.MaxValue));
                    }
                }

                this.Set(() => this.Salt, value);
            }
        }

        #endregion Properties (3)

        #region Methods (5)

        /// <summary>
        /// Creates a crypto stream.
        /// </summary>
        /// <param name="baseStream">The base stream.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The created stream.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="baseStream" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="mode" /> is not supported yet.
        /// </exception>
        protected CryptoStream CreateCryptoStream(Stream baseStream, CryptoStreamMode mode)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException("baseStream");
            }

            ICryptoTransform transform;

            using (var alg = Rijndael.Create())
            {
                using (var db = new Rfc2898DeriveBytes(this.Key, this.Salt, (int)this.Iterations))
                {
                    alg.Key = db.GetBytes(32);
                    alg.IV = db.GetBytes(16);

                    switch (mode)
                    {
                        case CryptoStreamMode.Read:
                            transform = alg.CreateDecryptor();
                            break;

                        case CryptoStreamMode.Write:
                            transform = alg.CreateEncryptor();
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            return new CryptoStream(baseStream, transform, mode);
        }

        /// <summary>
        /// <see cref="ICrypter.Decrypt(IEnumerable{byte})" />
        /// </summary>
        public byte[] Decrypt(IEnumerable<byte> crypted)
        {
            using (var cryptedStream = new MemoryStream(crypted.AsArray() ?? new byte[0], false))
            {
                using (var cs = this.CreateCryptoStream(cryptedStream, CryptoStreamMode.Read))
                {
                    using (var uncryptedStream = new MemoryStream())
                    {
                        cs.CopyTo(uncryptedStream);

                        return uncryptedStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="ICrypter.Encrypt(IEnumerable{byte})" />
        /// </summary>
        public byte[] Encrypt(IEnumerable<byte> uncrypted)
        {
            using (var uncryptedStream = new MemoryStream(uncrypted.AsArray() ?? new byte[0], false))
            {
                using (var cryptedStream = new MemoryStream())
                {
                    using (var cs = this.CreateCryptoStream(cryptedStream, CryptoStreamMode.Write))
                    {
                        uncryptedStream.CopyTo(cs);
                        cs.FlushFinalBlock();

                        return cryptedStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Tries to create a new instance from parameters.
        /// </summary>
        /// <param name="parameters">The parameter data.</param>
        /// <returns>
        /// The new instance or <see langword="null" /> if parameter data is invalid.
        /// </returns>
        public static RijndaelCrypter FromParameters(IEnumerable<byte> parameters)
        {
            using (var temp = new MemoryStream(parameters.AsArray() ?? new byte[0], false))
            {
                byte[] buffer;

                buffer = new byte[2];
                if (temp.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return null;
                }

                // key size
                var keySize = buffer.ToUInt16().Value;

                // key
                var key = new byte[keySize];
                if (temp.Read(key, 0, key.Length) != key.Length)
                {
                    return null;
                }

                buffer = new byte[2];
                if (temp.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return null;
                }

                // salt size
                var saltSize = buffer.ToUInt16().Value;

                // salt
                var salt = new byte[saltSize];
                if (temp.Read(salt, 0, salt.Length) != salt.Length)
                {
                    return null;
                }

                buffer = new byte[4];
                if (temp.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return null;
                }

                // iterations
                var iterations = buffer.ToUInt32().Value;
                if (iterations > int.MaxValue)
                {
                    return null;
                }

                return new RijndaelCrypter()
                {
                    Iterations = iterations,
                    Key = key,
                    Salt = salt,
                };
            }
        }

        /// <summary>
        /// Returns the parameters.
        /// </summary>
        /// <returns>The parameters.</returns>
        public byte[] ExportParameters()
        {
            using (var temp = new MemoryStream())
            {
                var key = this.Key ?? new byte[0];
                var keySize = (ushort)key.Length;

                var salt = this.Salt ?? new byte[0];
                var saltSize = (ushort)salt.Length;

                var iterations = (uint)this.Iterations;

                // key
                temp.Write(keySize.GetBytes(), 0, 2);
                temp.Write(key, 0, key.Length);

                // salt
                temp.Write(saltSize.GetBytes(), 0, 2);
                temp.Write(salt, 0, salt.Length);

                // iterations
                temp.Write(iterations.GetBytes(), 0, 4);

                return temp.ToArray();
            }
        }

        #endregion Methods (5)
    }
}