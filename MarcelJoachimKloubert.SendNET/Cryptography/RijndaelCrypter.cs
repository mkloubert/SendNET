// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace MarcelJoachimKloubert.SendNET.Cryptography
{
    public sealed class RijndaelCrypter : ICrypter
    {
        #region Fields (8)

        private int _iterations;
        private byte[] _pwd;
        private byte[] _salt;

        /// <summary>
        /// Attribute name for crypter type.
        /// </summary>
        public const string XML_ATTRIBUTE_NAME_CRYPTER_TYPE = "type";

        /// <summary>
        /// Element name for iteration count.
        /// </summary>
        public const string XML_ELEMENT_NAME_ITERATIONS = "iterations";

        /// <summary>
        /// Element name for password.
        /// </summary>
        public const string XML_ELEMENT_NAME_PASSWORD = "password";

        /// <summary>
        /// Element name for root element.
        /// </summary>
        public const string XML_ELEMENT_NAME_ROOT = "crypter";

        /// <summary>
        /// Element name for salt.
        /// </summary>
        public const string XML_ELEMENT_NAME_SALT = "salt";

        #endregion Fields (8)

        #region Constructors (2)

        public RijndaelCrypter(byte[] pwd, byte[] salt, int iteratations)
        {
            this._pwd = pwd;
            this._salt = salt;
            this._iterations = iteratations;
        }

        ~RijndaelCrypter()
        {
            this.Dispose(false);
        }

        #endregion Constructors (2)

        #region Methods (8)

        private CryptoStream CreateCryptoStream(Stream baseStream, CryptoStreamMode mode)
        {
            ICryptoTransform transform = null;

            using (var alg = Rijndael.Create())
            {
                var rdb = new Rfc2898DeriveBytes(this._pwd, this._salt, this._iterations);
                alg.Key = rdb.GetBytes(32);
                alg.IV = rdb.GetBytes(16);

                switch (mode)
                {
                    case CryptoStreamMode.Read:
                        transform = alg.CreateDecryptor();
                        break;

                    case CryptoStreamMode.Write:
                        transform = alg.CreateEncryptor();
                        break;
                }
            }

            return new CryptoStream(baseStream, transform, mode);
        }

        /// <inheriteddoc />
        public void Decrypt(Stream src, Stream dest)
        {
            var cs = this.CreateCryptoStream(src, CryptoStreamMode.Read);

            cs.CopyTo(dest);
        }

        /// <inheriteddoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this._pwd = new byte[this._pwd.Length];
            this._salt = new byte[this._salt.Length];
            this._iterations = 0;
        }

        /// <inheriteddoc />
        public void Encrypt(Stream src, Stream dest)
        {
            var cs = this.CreateCryptoStream(dest, CryptoStreamMode.Write);

            src.CopyTo(cs);
            cs.FlushFinalBlock();
        }

        /// <summary>
        /// Creates a new instance from XML data.
        /// </summary>
        /// <param name="xml">The XML data.</param>
        /// <returns>The new instance.</returns>
        public static RijndaelCrypter FromXml(XElement xml)
        {
            var pwdElement = xml.Element(XML_ELEMENT_NAME_PASSWORD);
            var saltElement = xml.Element(XML_ELEMENT_NAME_SALT);
            var iterationsElement = xml.Element(XML_ELEMENT_NAME_ITERATIONS);

            return new RijndaelCrypter(Convert.FromBase64String(pwdElement.Value.Trim()),
                                       Convert.FromBase64String(saltElement.Value.Trim()),
                                       int.Parse(iterationsElement.Value.Trim()));
        }

        /// <inheriteddoc />
        public XElement ToXml()
        {
            var result = new XElement(XML_ELEMENT_NAME_ROOT);
            result.SetAttributeValue(XML_ATTRIBUTE_NAME_CRYPTER_TYPE, this.GetType().FullName);

            // password
            var pwdElement = new XElement(XML_ELEMENT_NAME_PASSWORD);
            {
                pwdElement.Value = Convert.ToBase64String(this._pwd);

                result.Add(pwdElement);
            }

            // salt
            var saltElement = new XElement(XML_ELEMENT_NAME_SALT);
            {
                saltElement.Value = Convert.ToBase64String(this._salt);

                result.Add(saltElement);
            }

            // iterations
            var iterationsElement = new XElement(XML_ELEMENT_NAME_ITERATIONS);
            {
                iterationsElement.Value = this._iterations.ToString();

                result.Add(iterationsElement);
            }

            return result;
        }

        string ICrypter.ToXml()
        {
            return this.ToXml().ToString();
        }

        #endregion Methods (10)
    }
}