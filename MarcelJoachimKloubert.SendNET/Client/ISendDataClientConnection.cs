// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Cryptography;
using System;

namespace MarcelJoachimKloubert.SendNET.Client
{
    /// <summary>
    /// Describes an object that handles a client connection for sending data.
    /// </summary>
    public interface ISendDataClientConnection : IDisposable
    {
        #region Properties (3)

        /// <summary>
        /// Gets the underlying crypter.
        /// </summary>
        ICrypter Crypter { get; }

        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        Guid Id { get; }

        #endregion Properties (3)

        #region Methods (2)

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Disconnection();

        /// <summary>
        /// Sends a local file.
        /// </summary>
        /// <param name="localPath">The path of the local file.</param>
        void SendFile(string localPath);

        #endregion Methods (2)
    }
}