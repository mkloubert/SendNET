// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System;
using System.IO;
using System.Security.Cryptography;

namespace MarcelJoachimKloubert.SendNET.Cryptography
{
    /// <summary>
    /// Describes an object that encrypts or decrypts data.
    /// </summary>
    public interface ICrypter : IDisposable
    {
        #region Methods (3)
        
        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The target stream.</param>
        void Decrypt(Stream src, Stream dest);

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The target stream.</param>
        void Encrypt(Stream src, Stream dest);

        /// <summary>
        /// Stores the settings to XML data.
        /// </summary>
        /// <returns>Settings as XML data.</returns>
        string ToXml();

        #endregion Methods (3)
    }
}