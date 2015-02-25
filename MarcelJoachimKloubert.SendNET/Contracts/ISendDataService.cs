// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System.IO;
using System.ServiceModel;

namespace MarcelJoachimKloubert.SendNET.Contracts
{
    /// <summary>
    /// Describes a service for sending data.
    /// </summary>
    [ServiceContract()]
    public interface ISendDataService
    {
        #region Methods (4)

        /// <summary>
        /// Starts connection to service.
        /// </summary>
        /// <param name="meta">XML data for initialization.</param>
        /// <returns>The XML answer.</returns>
        [OperationContract(IsOneWay = false)]
        byte[] Connect(byte[] meta);

        /// <summary>
        /// Disconnects.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Disconnect();

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name="src">The source data.</param>
        /// <returns>The result.</returns>
        [OperationContract(IsOneWay = true)]
        void SendFile(Stream src);

        /// <summary>
        /// Sets up the file to send.
        /// </summary>
        /// <param name="meta">The meta data.</param>
        [OperationContract(IsOneWay = false)]
        void SetupFile(byte[] meta);

        #endregion Methods (4)
    }
}