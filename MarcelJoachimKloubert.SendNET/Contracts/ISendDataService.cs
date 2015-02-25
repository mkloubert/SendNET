// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System.ServiceModel;

namespace MarcelJoachimKloubert.SendNET.Contracts
{
    /// <summary>
    /// Describes a service for sending data.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ISendDataService
    {
        #region Methods (5)

        /// <summary>
        /// Closes the current file.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        [OperationContract(IsOneWay = false)]
        byte[] CloseFile();

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
        /// <returns>The result of the operation.</returns>
        [OperationContract(IsOneWay = false)]
        byte[] Disconnect();

        /// <summary>
        /// Opens a new file.
        /// </summary>
        /// <param name="meta">The meta data.</param>
        /// <returns>The result of the operation.</returns>
        [OperationContract(IsOneWay = false)]
        byte[] OpenFile(byte[] meta);

        /// <summary>
        /// Writes to a file.
        /// </summary>
        /// <param name="meta">The meta data.</param>
        /// <returns>The result of the operation.</returns>
        [OperationContract(IsOneWay = false)]
        byte[] WriteFile(byte[] meta);

        #endregion Methods (5)
    }
}